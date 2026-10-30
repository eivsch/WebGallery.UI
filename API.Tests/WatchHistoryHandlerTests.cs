using MinimalGallery.API;
using MinimalGallery.API.Storage;

namespace MinimalGallery.API.Tests;

public class WatchHistoryHandlerTests : IDisposable
{
    readonly string _root = Path.Combine(Path.GetTempPath(), "wg-tests-" + Guid.NewGuid());

    public WatchHistoryHandlerTests()
    {
        Directory.CreateDirectory(_root);
        Globals.StoragePath = _root;
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public void Empty_history_for_user_without_plays()
    {
        Assert.Empty(WatchHistoryHandler.GetHistory("alice"));
    }

    [Fact]
    public void Lists_newest_first()
    {
        DateTime t = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        WatchHistoryHandler.RecordPlay("alice", "a1", "one.mp4", t);
        WatchHistoryHandler.RecordPlay("alice", "a1", "two.mp4", t.AddMinutes(1));
        WatchHistoryHandler.RecordPlay("alice", "a2", "three.mp4", t.AddMinutes(2));

        var names = WatchHistoryHandler.GetHistory("alice").Select(h => h.MediaName).ToList();

        Assert.Equal(["three.mp4", "two.mp4", "one.mp4"], names);
    }

    [Fact]
    public void Replay_upserts_and_moves_to_top()
    {
        DateTime t = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        WatchHistoryHandler.RecordPlay("alice", "a1", "one.mp4", t);
        WatchHistoryHandler.RecordPlay("alice", "a1", "two.mp4", t.AddMinutes(1));
        WatchHistoryHandler.RecordPlay("alice", "a1", "one.mp4", t.AddMinutes(5));

        var history = WatchHistoryHandler.GetHistory("alice");

        Assert.Equal(2, history.Count);
        Assert.Equal("one.mp4", history[0].MediaName);
        Assert.Equal(t.AddMinutes(5), history[0].LastPlayedUtc);
    }

    [Fact]
    public void Histories_are_isolated_per_user_and_stored_in_user_dir()
    {
        WatchHistoryHandler.RecordPlay("alice", "a1", "one.mp4");
        WatchHistoryHandler.RecordPlay("bob", "b1", "two.mp4");

        Assert.Equal("one.mp4", Assert.Single(WatchHistoryHandler.GetHistory("alice")).MediaName);
        Assert.Equal("two.mp4", Assert.Single(WatchHistoryHandler.GetHistory("bob")).MediaName);
        Assert.True(File.Exists(Path.Combine(_root, "alice", "alice_watch_history.db")));
    }

    [Fact]
    public void Paging_is_applied()
    {
        DateTime t = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        for (int i = 0; i < 5; i++) WatchHistoryHandler.RecordPlay("alice", "a", $"{i}.mp4", t.AddMinutes(i));

        var page = WatchHistoryHandler.GetHistory("alice", from: 1, size: 2);

        Assert.Equal(["3.mp4", "2.mp4"], page.Select(h => h.MediaName));
    }
}
