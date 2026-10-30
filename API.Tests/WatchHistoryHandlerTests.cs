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
        WatchHistoryHandler.RecordPlay("alice", "a1", "one.mp4", playedAtUtc: t);
        WatchHistoryHandler.RecordPlay("alice", "a1", "two.mp4", playedAtUtc: t.AddMinutes(1));
        WatchHistoryHandler.RecordPlay("alice", "a2", "three.mp4", playedAtUtc: t.AddMinutes(2));

        var names = WatchHistoryHandler.GetHistory("alice").Select(h => h.MediaName).ToList();

        Assert.Equal(["three.mp4", "two.mp4", "one.mp4"], names);
    }

    [Fact]
    public void Replay_upserts_and_moves_to_top()
    {
        DateTime t = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        WatchHistoryHandler.RecordPlay("alice", "a1", "one.mp4", playedAtUtc: t);
        WatchHistoryHandler.RecordPlay("alice", "a1", "two.mp4", playedAtUtc: t.AddMinutes(1));
        WatchHistoryHandler.RecordPlay("alice", "a1", "one.mp4", playedAtUtc: t.AddMinutes(5));

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
    public void Media_id_is_stored_and_kept_on_replay_without_id()
    {
        WatchHistoryHandler.RecordPlay("alice", "a1", "one.mp4", "id-1");
        WatchHistoryHandler.RecordPlay("alice", "a1", "one.mp4");

        Assert.Equal("id-1", Assert.Single(WatchHistoryHandler.GetHistory("alice")).MediaId);
    }

    [Fact]
    public void Legacy_database_without_media_id_column_is_migrated()
    {
        string dir = Path.Combine(_root, "legacy");
        Directory.CreateDirectory(dir);
        using (var c = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={Path.Combine(dir, "legacy_watch_history.db")};Pooling=False"))
        {
            c.Open();
            var cmd = c.CreateCommand();
            cmd.CommandText = "CREATE TABLE watch_history (album TEXT NOT NULL, media_name TEXT NOT NULL, last_played_utc TEXT NOT NULL, PRIMARY KEY (album, media_name)); INSERT INTO watch_history VALUES ('a','old.mp4','2025-01-01T00:00:00.0000000Z');";
            cmd.ExecuteNonQuery();
        }

        WatchHistoryHandler.RecordPlay("legacy", "a", "new.mp4", "id-2");

        var history = WatchHistoryHandler.GetHistory("legacy");
        Assert.Equal(2, history.Count);
        Assert.Null(history.Single(h => h.MediaName == "old.mp4").MediaId);
        Assert.Equal("id-2", history.Single(h => h.MediaName == "new.mp4").MediaId);
    }

    [Fact]
    public void Play_count_increments_on_each_play()
    {
        WatchHistoryHandler.RecordPlay("alice", "a1", "one.mp4");
        WatchHistoryHandler.RecordPlay("alice", "a1", "one.mp4");
        WatchHistoryHandler.RecordPlay("alice", "a1", "two.mp4");

        var history = WatchHistoryHandler.GetHistory("alice");

        Assert.Equal(2, history.Single(h => h.MediaName == "one.mp4").PlayCount);
        Assert.Equal(1, history.Single(h => h.MediaName == "two.mp4").PlayCount);
    }

    [Fact]
    public void Position_is_saved_for_existing_entry_only()
    {
        Assert.False(WatchHistoryHandler.SavePosition("alice", "a1", "one.mp4", 10, 100));
        WatchHistoryHandler.RecordPlay("alice", "a1", "one.mp4");

        Assert.True(WatchHistoryHandler.SavePosition("alice", "a1", "one.mp4", 42.5, 100));
        Assert.False(WatchHistoryHandler.SavePosition("alice", "a1", "other.mp4", 5, 100));

        var entry = Assert.Single(WatchHistoryHandler.GetHistory("alice"));
        Assert.Equal(42.5, entry.PositionSeconds);
        Assert.Equal(100, entry.DurationSeconds);
        Assert.False(entry.Watched);
        Assert.Equal(42.5, WatchHistoryHandler.GetEntry("alice", "a1", "one.mp4")!.PositionSeconds);
    }

    [Fact]
    public void Position_past_90_percent_marks_watched_and_resets_position()
    {
        WatchHistoryHandler.RecordPlay("alice", "a1", "one.mp4");
        WatchHistoryHandler.SavePosition("alice", "a1", "one.mp4", 91, 100);

        var entry = WatchHistoryHandler.GetEntry("alice", "a1", "one.mp4")!;
        Assert.True(entry.Watched);
        Assert.Equal(0, entry.PositionSeconds);

        WatchHistoryHandler.SavePosition("alice", "a1", "one.mp4", 20, 100);
        Assert.False(WatchHistoryHandler.GetEntry("alice", "a1", "one.mp4")!.Watched);
    }

    [Fact]
    public void Saving_position_does_not_change_order_or_play_count()
    {
        DateTime t = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        WatchHistoryHandler.RecordPlay("alice", "a1", "one.mp4", playedAtUtc: t);
        WatchHistoryHandler.RecordPlay("alice", "a1", "two.mp4", playedAtUtc: t.AddMinutes(1));
        WatchHistoryHandler.SavePosition("alice", "a1", "one.mp4", 5, 100);

        var history = WatchHistoryHandler.GetHistory("alice");

        Assert.Equal(["two.mp4", "one.mp4"], history.Select(h => h.MediaName));
        Assert.Equal(1, history[1].PlayCount);
    }

    [Fact]
    public void Remove_deletes_single_entry()
    {
        WatchHistoryHandler.RecordPlay("alice", "a1", "one.mp4");
        WatchHistoryHandler.RecordPlay("alice", "a1", "two.mp4");

        Assert.True(WatchHistoryHandler.Remove("alice", "a1", "one.mp4"));
        Assert.False(WatchHistoryHandler.Remove("alice", "a1", "one.mp4"));

        Assert.Equal("two.mp4", Assert.Single(WatchHistoryHandler.GetHistory("alice")).MediaName);
    }

    [Fact]
    public void Clear_removes_all_entries_for_user_only()
    {
        WatchHistoryHandler.RecordPlay("alice", "a1", "one.mp4");
        WatchHistoryHandler.RecordPlay("bob", "b1", "two.mp4");

        WatchHistoryHandler.Clear("alice");
        WatchHistoryHandler.Clear("nobody");

        Assert.Empty(WatchHistoryHandler.GetHistory("alice"));
        Assert.Single(WatchHistoryHandler.GetHistory("bob"));
    }

    [Fact]
    public void Legacy_database_gets_default_play_count_and_position()
    {
        string dir = Path.Combine(_root, "legacy2");
        Directory.CreateDirectory(dir);
        using (var c = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={Path.Combine(dir, "legacy2_watch_history.db")};Pooling=False"))
        {
            c.Open();
            var cmd = c.CreateCommand();
            cmd.CommandText = "CREATE TABLE watch_history (album TEXT NOT NULL, media_name TEXT NOT NULL, media_id TEXT NULL, last_played_utc TEXT NOT NULL, PRIMARY KEY (album, media_name)); INSERT INTO watch_history VALUES ('a','old.mp4',NULL,'2025-01-01T00:00:00.0000000Z');";
            cmd.ExecuteNonQuery();
        }

        var item = Assert.Single(WatchHistoryHandler.GetHistory("legacy2"));

        Assert.Equal(1, item.PlayCount);
        Assert.Equal(0, item.PositionSeconds);
        Assert.False(item.Watched);
    }

    [Fact]
    public void Paging_is_applied()
    {
        DateTime t = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        for (int i = 0; i < 5; i++) WatchHistoryHandler.RecordPlay("alice", "a", $"{i}.mp4", playedAtUtc: t.AddMinutes(i));

        var page = WatchHistoryHandler.GetHistory("alice", from: 1, size: 2);

        Assert.Equal(["3.mp4", "2.mp4"], page.Select(h => h.MediaName));
    }
}
