using Microsoft.Data.Sqlite;
using MinimalGallery.API.Models;

namespace MinimalGallery.API.Storage;

// One SQLite file per user, stored in the user's own directory so it is removed together with the user.
static class WatchHistoryHandler
{
    public static void RecordPlay(string username, string album, string mediaName, DateTime? playedAtUtc = null)
    {
        using SqliteConnection connection = Open(username);
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO watch_history (album, media_name, last_played_utc)
            VALUES ($album, $media, $played)
            ON CONFLICT(album, media_name) DO UPDATE SET last_played_utc = excluded.last_played_utc;
            """;
        cmd.Parameters.AddWithValue("$album", album);
        cmd.Parameters.AddWithValue("$media", mediaName);
        cmd.Parameters.AddWithValue("$played", (playedAtUtc ?? DateTime.UtcNow).ToUniversalTime().ToString("O"));
        cmd.ExecuteNonQuery();
    }

    public static List<WatchHistoryItem> GetHistory(string username, int from = 0, int size = 100)
    {
        List<WatchHistoryItem> result = [];
        if (!File.Exists(GetPath(username))) return result;

        using SqliteConnection connection = Open(username);
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT album, media_name, last_played_utc FROM watch_history
            ORDER BY last_played_utc DESC, rowid DESC
            LIMIT $size OFFSET $from;
            """;
        cmd.Parameters.AddWithValue("$size", size);
        cmd.Parameters.AddWithValue("$from", from);
        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new WatchHistoryItem
            {
                Album = reader.GetString(0),
                MediaName = reader.GetString(1),
                LastPlayedUtc = DateTime.Parse(reader.GetString(2), null, System.Globalization.DateTimeStyles.RoundtripKind),
            });
        }

        return result;
    }

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
                last_played_utc TEXT NOT NULL,
                PRIMARY KEY (album, media_name)
            );
            """;
        cmd.ExecuteNonQuery();

        return connection;
    }

    static string GetPath(string username) =>
        Path.Combine(Globals.StoragePath, username, $"{username}_watch_history.db");
}
