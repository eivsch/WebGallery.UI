using Microsoft.Data.Sqlite;
using MinimalGallery.API.Models;

namespace MinimalGallery.API.Storage;

// One SQLite file per user, stored in the user's own directory so it is removed together with the user.
static class WatchHistoryHandler
{
    public static void RecordPlay(string username, string album, string mediaName, string? mediaId = null, DateTime? playedAtUtc = null)
    {
        using SqliteConnection connection = Open(username);
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO watch_history (album, media_name, media_id, last_played_utc, play_count)
            VALUES ($album, $media, $id, $played, 1)
            ON CONFLICT(album, media_name) DO UPDATE SET
                last_played_utc = excluded.last_played_utc,
                play_count = play_count + 1,
                media_id = COALESCE(excluded.media_id, media_id);
            """;
        cmd.Parameters.AddWithValue("$album", album);
        cmd.Parameters.AddWithValue("$media", mediaName);
        cmd.Parameters.AddWithValue("$id", (object?)mediaId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$played", (playedAtUtc ?? DateTime.UtcNow).ToUniversalTime().ToString("O"));
        cmd.ExecuteNonQuery();

        // Retention: keep only the most recently played entries.
        cmd.Parameters.Clear();
        cmd.CommandText = """
            DELETE FROM watch_history WHERE rowid NOT IN (
                SELECT rowid FROM watch_history ORDER BY last_played_utc DESC, rowid DESC LIMIT $max);
            """;
        cmd.Parameters.AddWithValue("$max", MaxEntries);
        cmd.ExecuteNonQuery();
    }

    public const int MaxEntries = 500;

    public static List<WatchHistoryItem> GetHistory(string username, int from = 0, int size = 100)
    {
        from = Math.Max(0, from);
        size = Math.Clamp(size, 1, MaxEntries);
        List<WatchHistoryItem> result = [];
        if (!File.Exists(GetPath(username))) return result;

        using SqliteConnection connection = Open(username);
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT album, media_name, last_played_utc, media_id, play_count, position_seconds, duration_seconds, watched FROM watch_history
            ORDER BY last_played_utc DESC, rowid DESC
            LIMIT $size OFFSET $from;
            """;
        cmd.Parameters.AddWithValue("$size", size);
        cmd.Parameters.AddWithValue("$from", from);
        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read()) result.Add(Map(reader));

        return result;
    }

    public static WatchHistoryItem? GetEntry(string username, string album, string mediaName)
    {
        if (!File.Exists(GetPath(username))) return null;

        using SqliteConnection connection = Open(username);
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT album, media_name, last_played_utc, media_id, play_count, position_seconds, duration_seconds, watched FROM watch_history
            WHERE album = $album AND media_name = $media;
            """;
        cmd.Parameters.AddWithValue("$album", album);
        cmd.Parameters.AddWithValue("$media", mediaName);
        using SqliteDataReader reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    // Only updates existing entries: positions never add a video to the history, only play events do.
    // Reaching 90% of the duration marks the entry as watched and resets the position so it restarts.
    public static bool SavePosition(string username, string album, string mediaName, double positionSeconds, double? durationSeconds)
    {
        if (!File.Exists(GetPath(username))) return false;

        bool watched = durationSeconds is > 0 && positionSeconds >= durationSeconds * WatchedThreshold;
        using SqliteConnection connection = Open(username);
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE watch_history SET position_seconds = $pos, duration_seconds = $dur, watched = $watched
            WHERE album = $album AND media_name = $media;
            """;
        cmd.Parameters.AddWithValue("$pos", watched ? 0 : Math.Max(0, positionSeconds));
        cmd.Parameters.AddWithValue("$dur", (object?)durationSeconds ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$watched", watched ? 1 : 0);
        cmd.Parameters.AddWithValue("$album", album);
        cmd.Parameters.AddWithValue("$media", mediaName);
        return cmd.ExecuteNonQuery() > 0;
    }

    public static bool Remove(string username, string album, string mediaName)
    {
        if (!File.Exists(GetPath(username))) return false;

        using SqliteConnection connection = Open(username);
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM watch_history WHERE album = $album AND media_name = $media;";
        cmd.Parameters.AddWithValue("$album", album);
        cmd.Parameters.AddWithValue("$media", mediaName);
        return cmd.ExecuteNonQuery() > 0;
    }

    public static void Clear(string username)
    {
        if (!File.Exists(GetPath(username))) return;

        using SqliteConnection connection = Open(username);
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM watch_history;";
        cmd.ExecuteNonQuery();
    }

    const double WatchedThreshold = 0.9;

    static WatchHistoryItem Map(SqliteDataReader reader) => new()
    {
        Album = reader.GetString(0),
        MediaName = reader.GetString(1),
        LastPlayedUtc = DateTime.Parse(reader.GetString(2), null, System.Globalization.DateTimeStyles.RoundtripKind),
        MediaId = reader.IsDBNull(3) ? null : reader.GetString(3),
        PlayCount = reader.GetInt32(4),
        PositionSeconds = reader.GetDouble(5),
        DurationSeconds = reader.IsDBNull(6) ? null : reader.GetDouble(6),
        Watched = reader.GetInt32(7) != 0,
    };

    static SqliteConnection Open(string username)
    {
        string path = GetPath(username);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        // Pooling is disabled so the file handle is released and a user directory can be deleted.
        SqliteConnection connection = new($"Data Source={path};Pooling=False");
        connection.Open();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS watch_history (
                album TEXT NOT NULL,
                media_name TEXT NOT NULL,
                media_id TEXT NULL,
                last_played_utc TEXT NOT NULL,
                PRIMARY KEY (album, media_name)
            );
            """;
        cmd.ExecuteNonQuery();

        // Databases created by older versions need the newer columns added.
        EnsureColumn(connection, "media_id", "TEXT NULL");
        EnsureColumn(connection, "play_count", "INTEGER NOT NULL DEFAULT 1");
        EnsureColumn(connection, "position_seconds", "REAL NOT NULL DEFAULT 0");
        EnsureColumn(connection, "duration_seconds", "REAL NULL");
        EnsureColumn(connection, "watched", "INTEGER NOT NULL DEFAULT 0");

        return connection;
    }

    static void EnsureColumn(SqliteConnection connection, string column, string definition)
    {
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('watch_history') WHERE name = $name;";
        cmd.Parameters.AddWithValue("$name", column);
        if ((long)cmd.ExecuteScalar()! != 0) return;

        cmd.CommandText = $"ALTER TABLE watch_history ADD COLUMN {column} {definition};";
        cmd.ExecuteNonQuery();
    }

    static string GetPath(string username) =>
        Path.Combine(Globals.StoragePath, username, $"{username}_watch_history.db");
}
