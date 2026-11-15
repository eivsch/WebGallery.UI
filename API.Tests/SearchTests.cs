using MinimalGallery.API.Models;
using MinimalGallery.API.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MinimalGallery.API.Tests;

[CollectionDefinition("Storage", DisableParallelization = true)]
public class StorageCollection;

[Collection("Storage")]
public class SearchTests : IDisposable
{
    const long LongVideoSize = 104857600;
    readonly string _root = Path.Combine(Path.GetTempPath(), "wg-search-tests-" + Guid.NewGuid());

    public SearchTests()
    {
        Directory.CreateDirectory(_root);
        Globals.StoragePath = _root;
        UserMetaHandler.CreateNewUser(new() { Username = "alice", Password = "test" });
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Theory]
    [InlineData("", null)]
    [InlineData("&unwatchedOnly=true&random=true", true)]
    [InlineData("&unwatchedOnly=false&random=false", false)]
    public async Task Search_query_flags_are_optional_and_bind_without_breaking_existing_clients(string flags, bool? expected)
    {
        SearchRequest? boundRequest = null;
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        var endpoint = RequestDelegateFactory.Create(([AsParameters] SearchRequest request) =>
        {
            boundRequest = request;
            return Results.Ok();
        });
        var context = new DefaultHttpContext
        {
            RequestServices = services
        };
        context.Request.Method = "GET";
        context.Request.QueryString = new QueryString("?maxResults=1&allTagsMustMatch=false" + flags);

        await endpoint.RequestDelegate(context);

        Assert.Equal(200, context.Response.StatusCode);
        Assert.NotNull(boundRequest);
        Assert.Equal(expected, boundRequest.UnwatchedOnly);
        Assert.Equal(expected, boundRequest.Random);
    }

    [Fact]
    public void Random_selection_considers_every_match_beyond_1000_and_in_later_albums()
    {
        AddAlbum("early", Enumerable.Range(0, 1005).Select(i => Video($"early-{i}")));
        AddAlbum("late", Enumerable.Range(0, 205).Select(i => Video($"late-{i}")));
        var random = new ReplaceWithLatestRandom();

        var hit = Assert.Single(RequestHelper.Search("alice", LongUnwatched(random: true), randomSource: random)!);

        Assert.Equal("late-204", hit.MediaItem.Id);
        Assert.Equal("late", hit.AlbumName);
        Assert.Equal(204, hit.MediaAlbumIndex);
        Assert.Equal(1209, random.Bounds.Count);
        Assert.Equal(Enumerable.Range(2, 1209).Select(i => (long)i), random.Bounds);
    }

    [Fact]
    public void Long_unwatched_search_uses_size_boundary_and_no_tag_requirement()
    {
        Media tagged = Video("tagged");
        tagged.Tags.Add(new() { TagName = "unwatched", Created = DateTime.UtcNow });
        AddAlbum("a", [
            Video("boundary"),
            tagged,
            Video("short") with { Size = LongVideoSize - 1 },
            Video("unknown") with { Size = null },
            Video("image") with { Name = "image.jpg" },
            Video("uppercase") with { Name = "uppercase.MP4" }
        ]);

        var hits = RequestHelper.Search("alice", LongUnwatched() with { MaxResults = 20 })!;

        Assert.Equal(["boundary", "tagged", "uppercase"], hits.Select(h => h.MediaItem.Id));
    }

    [Fact]
    public void Any_recorded_play_excludes_a_video_regardless_of_completion_or_tag()
    {
        AddAlbum("a", [Video("partial"), Video("complete"), Video("played"), Video("unplayed")]);
        WatchHistoryHandler.RecordPlay("alice", "a", "partial.mp4", "partial");
        WatchHistoryHandler.SavePosition("alice", "a", "partial.mp4", 10, 100);
        WatchHistoryHandler.RecordPlay("alice", "a", "complete.mp4", "complete");
        WatchHistoryHandler.SavePosition("alice", "a", "complete.mp4", 95, 100);
        WatchHistoryHandler.RecordPlay("alice", "a", "played.mp4", "played");
        RequestHelper.AddTag("alice", "a", "played", new() { TagName = "unwatched" });

        var hit = Assert.Single(RequestHelper.Search("alice", LongUnwatched(random: true))!);

        Assert.Equal("unplayed", hit.MediaItem.Id);
    }

    [Fact]
    public void Played_ids_survive_path_changes_and_legacy_entries_match_exact_album_and_name()
    {
        AddAlbum("a", [
            Video("moved") with { Name = "renamed.mp4" },
            Video("legacy"),
            Video("replacement") with { Name = "reused.mp4" }
        ]);
        AddAlbum("b", [Video("other") with { Name = "legacy.mp4" }]);
        WatchHistoryHandler.RecordPlay("alice", "old-album", "old-name.mp4", "moved");
        WatchHistoryHandler.RecordPlay("alice", "a", "legacy.mp4");
        WatchHistoryHandler.RecordPlay("alice", "a", "reused.mp4", "deleted-id");

        var hits = RequestHelper.Search("alice", LongUnwatched() with { MaxResults = 20 })!;

        Assert.Equal(["replacement", "other"], hits.Select(h => h.MediaItem.Id));
    }

    [Fact]
    public void Full_history_is_checked_beyond_both_default_and_maximum_page_sizes()
    {
        AddAlbum("a", [Video("oldest"), Video("unplayed")]);
        DateTime start = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        WatchHistoryHandler.RecordPlay("alice", "a", "oldest.mp4", "oldest", start);
        for (int i = 0; i < 605; i++)
            WatchHistoryHandler.RecordPlay("alice", "a", $"other-{i}.mp4", $"other-{i}", start.AddMinutes(i + 1));

        var hit = Assert.Single(RequestHelper.Search("alice", LongUnwatched(random: true))!);

        Assert.Equal("unplayed", hit.MediaItem.Id);
    }

    [Fact]
    public void Removing_and_clearing_history_restore_eligibility_without_affecting_other_users()
    {
        AddAlbum("a", [Video("one"), Video("two")]);
        WatchHistoryHandler.RecordPlay("alice", "a", "one.mp4", "one");
        WatchHistoryHandler.RecordPlay("alice", "a", "two.mp4", "two");
        WatchHistoryHandler.RecordPlay("bob", "a", "one.mp4", "one");
        var request = LongUnwatched(random: true);
        Assert.Empty(RequestHelper.Search("alice", request)!);

        WatchHistoryHandler.Remove("alice", "a", "one.mp4");
        Assert.Equal("one", Assert.Single(RequestHelper.Search("alice", request)!).MediaItem.Id);

        WatchHistoryHandler.Clear("alice");
        Assert.Equal(2, RequestHelper.Search("alice", request with { MaxResults = 20 })!.Count);
        Assert.Single(WatchHistoryHandler.GetHistory("bob"));
    }

    [Fact]
    public void Random_sampling_keeps_requested_count_and_skips_before_sampling()
    {
        AddAlbum("a", Enumerable.Range(0, 10).Select(i => Video($"v{i}")));
        var random = new ReplaceWithLatestRandom();
        var request = LongUnwatched(random: true) with { MaxResults = 2, HitsToSkip = 2 };

        var hits = RequestHelper.Search("alice", request, randomSource: random)!;

        Assert.Equal(2, hits.Count);
        Assert.Equal(["v9", "v3"], hits.Select(h => h.MediaItem.Id));
        Assert.Equal(Enumerable.Range(3, 6).Select(i => (long)i), random.Bounds);
    }

    [Fact]
    public void Ordinary_search_preserves_order_pagination_and_includes_played_media()
    {
        AddAlbum("a", Enumerable.Range(0, 205).Select(i => Video($"v{i}")));
        WatchHistoryHandler.RecordPlay("alice", "a", "v201.mp4", "v201");
        var request = new SearchRequest { FileExtensions = "mp4", HitsToSkip = 201, MaxResults = 2 };

        var hits = RequestHelper.Search("alice", request)!;

        Assert.Equal(["v201", "v202"], hits.Select(h => h.MediaItem.Id));
        Assert.Equal([201, 202], hits.Select(h => h.MediaAlbumIndex));
    }

    [Fact]
    public void Random_search_applies_existing_album_name_date_and_tag_filters()
    {
        AddAlbum("a", [
            Video("match"),
            Video("match-old") with { Created = DateTimeOffset.UtcNow.AddYears(-1) },
            Video("match-future") with { Created = DateTimeOffset.UtcNow.AddYears(1) },
            Video("match-no-tag"),
            Video("wrong-name")
        ]);
        AddAlbum("b", [Video("match-other-album")]);
        foreach (string id in new[] { "match", "match-old", "match-future", "wrong-name" })
            RequestHelper.AddTag("alice", "a", id, new() { TagName = "selected" });
        RequestHelper.AddTag("alice", "b", "match-other-album", new() { TagName = "selected" });
        var request = LongUnwatched(random: true) with
        {
            Albums = "a", Tags = "selected", MediaNameContains = "match"
        };

        var hit = Assert.Single(RequestHelper.Search("alice", request,
            createdAfter: DateTimeOffset.UtcNow.AddDays(-1), createdBefore: DateTimeOffset.UtcNow.AddDays(1))!);

        Assert.Equal("match", hit.MediaItem.Id);
    }

    static SearchRequest LongUnwatched(bool random = false) => new()
    {
        FileExtensions = "mp4", MinFileSize = LongVideoSize,
        UnwatchedOnly = true, Random = random, MaxResults = 1
    };

    static Media Video(string id) => new()
    {
        Id = id, Name = id + ".mp4", Size = LongVideoSize, Created = DateTimeOffset.UtcNow
    };

    static void AddAlbum(string name, IEnumerable<Media> items)
    {
        RequestHelper.CreateNewAlbum("alice", name);
        foreach (Media item in items) AlbumIndexHandler.AddMedia("alice", name, item);
    }

    sealed class ReplaceWithLatestRandom : Random
    {
        public List<long> Bounds { get; } = [];

        public override long NextInt64(long maxValue)
        {
            Bounds.Add(maxValue);
            return 0;
        }
    }
}
