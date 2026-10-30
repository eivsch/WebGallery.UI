using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using MinimalGallery.API.Models;

namespace MinimalGallery.API.Storage;

static class AlbumIndexHandler
{
    const int CHUNK_SIZE = 1024*2;
    const string END_TAG = "<END>";

    public static void CreateIndex(string username, string albumName)
    {
        if (!PathExists(Globals.StoragePath, username)) throw new Exception($"The user '{username}' doesn't exist. Create the user first.");
        string albumIndexName = GetPathAlbum(username, albumName);
        if (File.Exists(albumIndexName)) return;
        using FileStream fs = File.Create(albumIndexName);

        static bool PathExists(params string[] paths)
        {
            string path = Path.Combine(paths);
            return Directory.Exists(path);
        }
    }

    public static bool DeleteIndex(string username, string albumName)
    {
        string path = GetPathAlbum(username, albumName);
        if (!File.Exists(path)) return false;
        File.Delete(path);

        return true;
    }

    public static void AddMedia(string username, string albumName, Media media)
    {
        string path = GetPathAlbum(username, albumName);
        if (!File.Exists(path)) throw new Exception($"The album {albumName} doesn't exist. Create it first.");

        byte[] newData = GetDataAsByteChunk(media);
        using FileStream fs = new(path, FileMode.Open, FileAccess.Write);
        fs.Seek(0, SeekOrigin.End);
        fs.Write(newData, 0, newData.Length);
    }

    public static Media? GetMedia(string username, string albumName, string searchTerm)
    {
        string lineBegin = @"{""Id"":""";
        searchTerm = lineBegin + searchTerm;

        string path = GetPathAlbum(username, albumName);
        if (!File.Exists(path)) return null;

        int index = 0;
        foreach (string line in File.ReadLines(path))
        {
            if (line.StartsWith(searchTerm))
            {
                Media? media = DeserializeMediaString(line);
                if (media != null) media.Index = index;

                return media;
            }

            index++;
        }

        return null;
    }

    public static List<Media>? GetAlbumItems(string username, string albumName, int fromIndex = 0, int size = 32)
    {
        if (fromIndex < 0) throw new ArgumentException("Parameter 'from' must be 0 or above.");

        List<Media> result = [];

        string path = GetPathAlbum(username, albumName);
        if (!File.Exists(path)) return null;

        byte[] buffer = new byte[CHUNK_SIZE];
        using (FileStream fs = new(path, FileMode.Open, FileAccess.Read))
        {
            int endIndex = fromIndex + size;
            for (; fromIndex < endIndex; fromIndex++)
            {
                int offset = fromIndex * CHUNK_SIZE;
                if (offset > fs.Length - CHUNK_SIZE) break;

                fs.Seek(offset, SeekOrigin.Begin);
                int n = fs.Read(buffer, 0, CHUNK_SIZE);
                if (n == 0) break;
                string chunkStr = Encoding.UTF8.GetString(buffer);
                Media? m = DeserializeMediaString(chunkStr);
                if (m != null)
                {
                    m.Index = fromIndex;
                    result.Add(m);
                }
            }
        }

        return result;
    }

    public static (Media? m, int? i) FindMediaChunk(string username, string albumName, string searchTerm, int startChunk = 0, int endChunk = 10000)
    {
        string path = GetPathAlbum(username, albumName);
        if (!File.Exists(path)) return (null, null);

        int chunkIndex = startChunk;
        byte[] buffer = new byte[CHUNK_SIZE];
        using (FileStream fs = new(path, FileMode.Open, FileAccess.Read))
        {
            while (startChunk <= endChunk)
            {
                int offset = chunkIndex*CHUNK_SIZE;
                if (offset > fs.Length-CHUNK_SIZE) break;

                fs.Seek(offset, SeekOrigin.Begin);
                int n = fs.Read(buffer, 0, CHUNK_SIZE);
                string chunkStr = Encoding.UTF8.GetString(buffer);
                if (chunkStr.Contains(searchTerm))
                {
                    Media? m = DeserializeMediaString(chunkStr);

                    return (m, chunkIndex);
                }

                chunkIndex++;
            }
        }

        return (null, null);
    }

    public static void WriteMediaChunk(string username, string albumName, int chunkIndex, Media media)
    {
        string path = GetPathAlbum(username, albumName);
        if (!File.Exists(path)) return;

        using FileStream fs = new(path, FileMode.Open, FileAccess.Write);
        byte[] dataChunk = GetDataAsByteChunk(media);
        fs.Seek(chunkIndex*CHUNK_SIZE, SeekOrigin.Begin);
        fs.Write(dataChunk, 0, dataChunk.Length);
    }

    public static void DeleteMediaChunk(string username, string albumName, int i)
    {
        string path = GetPathAlbum(username, albumName);
        if (!File.Exists(path)) return;

        // Shift the remainder of the file one chunk "upwards"
        using FileStream fs = new(path, FileMode.Open, FileAccess.ReadWrite);
        int j = i + 1;
        int bytesToRead = (int)fs.Length - (j * CHUNK_SIZE);  // This will only work for medium size files. If file size expected to exceed 100 MB then should consider rewriting
        byte[] readBuffer = new byte[bytesToRead];
        // read remainder from 'j'
        int offset_j = j * CHUNK_SIZE;
        fs.Seek(offset_j, SeekOrigin.Begin);
        fs.Read(readBuffer, 0, readBuffer.Length);
        // overwrite remainder from 'i'
        int offset_i = i * CHUNK_SIZE;
        fs.Seek(offset_i, SeekOrigin.Begin);
        fs.Write(readBuffer, 0, readBuffer.Length);
        // remove remainder at the end
        long newLength = fs.Length - CHUNK_SIZE;
        fs.SetLength(newLength);
    }

    public static int GetAlbumItemsCount(string username, string albumName)
    {
        string path = GetPathAlbum(username, albumName);
        if (!File.Exists(path)) return -1;

        using FileStream fs = new(path, FileMode.Open, FileAccess.Read);
        int count =  (int) fs.Length/CHUNK_SIZE;

        return count;
    }

    public static void RenameAlbumIndexFile(string username, string albumName, string newName)
    {
        string path = GetPathAlbum(username, albumName);
        if (!File.Exists(path)) throw new Exception("The album to rename does not exist.");

        string newPath = GetPathAlbum(username, newName);
        File.Move(path, newPath);
    }

    public static void RebuildAlbumTags(string username, string albumName)
    {
        string path = GetPathAlbum(username, albumName);
        if (!File.Exists(path)) throw new Exception($"The album {albumName} doesn't exist. Create it first.");

        List<UserAlbumTagMeta> tags = new();
        foreach (string line in File.ReadLines(path))
        {
            Media? media = DeserializeMediaString(line);
            if (media == null) continue;

            foreach (Tag tag in media.Tags)
            {
                UserAlbumTagMeta? t = tags.FirstOrDefault(f => f.TagName == tag.TagName);
                if (t == null)
                {
                    t = new UserAlbumTagMeta
                    {
                        TagName = tag.TagName,
                        Count = 1
                    };
                    tags.Add(t);
                }
                else
                {
                    t.Count++;
                }
            }
        }

        UserMeta userMeta = UserMetaHandler.GetUserMeta(username) ?? throw new Exception($"The user '{username}' doesn't exist.");
        UserAlbumMeta? albumMeta = userMeta.AlbumMeta.FirstOrDefault(f => f.AlbumName == albumName) 
            ?? throw new Exception($"The album '{albumName}' doesn't exist for user '{username}'.");
        albumMeta.Tags = tags;
        
        UserMetaHandler.WriteUser(userMeta);
    }

    // helpers
    private static byte[] GetDataAsByteChunk(Media data)
    {
        byte[] dataChunk = new byte[CHUNK_SIZE];

        // serialize to json
        string json = JsonSerializer.Serialize(data);
        json += END_TAG;
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        if (jsonBytes.Length > CHUNK_SIZE - 2) throw new Exception($"Overflow error: Maximum chunk size for a media record is {CHUNK_SIZE - 2}. Current size is {jsonBytes.Length}.");
        jsonBytes.CopyTo(dataChunk, 0);

        // add a line ending after padding
        byte[] lineEnding = Encoding.UTF8.GetBytes("\n");
        byte l = lineEnding[0];
        dataChunk[dataChunk.Length - 1] = l;

        return dataChunk;
    }

    private static Media? DeserializeMediaString(string mediaStr)
    {
        int endIndex = mediaStr.IndexOf(END_TAG);
        mediaStr = mediaStr[..endIndex];
        Media? media = JsonSerializer.Deserialize<Media>(mediaStr);

        return media;
    }

    private static string GetPathAlbum(string username, string albumName)
    {
        string path = Path.Combine(Globals.StoragePath, username, $"{albumName}.{Globals.FILE_EXT}");

        return path;
    }
}