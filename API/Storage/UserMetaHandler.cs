using System.Text.Json;
using MinimalGallery.API.Models;

namespace MinimalGallery.API.Storage;

static class UserMetaHandler
{
    public static bool CreateNewUser(NewUserRequest r)
    {
        string path = Path.Combine(Globals.StoragePath, r.Username);
        if (Directory.Exists(path)) 
            return false;
        
        Directory.CreateDirectory(path);

        UserMeta metaData = new()
        {
            Username = r.Username,
            Password = r.Password,
            AlbumMeta = [],
            Created = DateTime.UtcNow,
        };

        WriteUser(metaData);

        return true;
    }

    public static UserMeta? GetUserMeta(string username)
    {
        string path = GetPathUser(username);
        if (!File.Exists(path)) return null;

        UserMeta? user = ReadUser(username);

        return user;
    }

    public static bool DeleteUserMeta(string username)
    {
        string path = Path.Combine(Globals.StoragePath, username);
        if (!Directory.Exists(path)) return false;
        
        Directory.Delete(path, recursive: true);

        return true;
    }

    public static void DeleteAlbumMeta(string username, string albumName)
    {
        UserMeta userMeta = ReadUser(username);
        UserAlbumMeta? m = userMeta.AlbumMeta.FirstOrDefault(f => f.AlbumName == albumName);
        if (m == null) return;

        userMeta.AlbumMeta.Remove(m);
        // List<Tag> albumTags = [];
        // foreach (string line in File.ReadLines(path))
        // {
        //     Media? media = DeserializeMediaString(line);
        //     if (media == null) continue;
        //     foreach (Tag tag in media.Tags)
        //     {
        //         UserAlbumTagMeta? tagMeta = userMeta.TagMeta.FirstOrDefault(x => x.TagName == tag.TagName);
        //         if (tagMeta == null) continue;
        //         else if (tagMeta.Count <= 1) userMeta.TagMeta.Remove(tagMeta);
        //         else tagMeta.Count--;
        //     }
        // }

        WriteUser(userMeta);
    }

    public static void RenameAlbumMeta(string username, string albumName, string newAlbumName)
    {
        UserMeta userMeta = ReadUser(username);
        UserAlbumMeta? albumMeta = userMeta.AlbumMeta.FirstOrDefault(f => f.AlbumName == albumName);
        if (albumMeta == null) return;

        albumMeta.AlbumName = newAlbumName;
        WriteUser(userMeta);
    }

    public static void HandleTagDeletion(string username, string albumName, List<Tag> tags)
    {
        UserMeta user = ReadUser(username);
        UserAlbumMeta albumMeta = user.AlbumMeta.Single(x => x.AlbumName == albumName);
        foreach (Tag tag in tags)
            UpdateTagMeta(albumMeta, tag);

        WriteUser(user);
    }

    public static void HandleMediaDeletion(string username, string albumName, Media media)
    {
        UserMeta user = ReadUser(username);
        UserAlbumMeta albumMeta = user.AlbumMeta.Single(x => x.AlbumName == albumName);
        foreach (Tag tag in media.Tags)
            UpdateTagMeta(albumMeta, tag);

        if (media.Likes > 0)
        {
            albumMeta.TotalLikes -= media.Likes;
            albumMeta.TotalUniqueLikes--;
        }

        WriteUser(user);
    }

    public static void AddTagMeta(string username, string albumName, Tag t)
    {
        UserMeta user = ReadUser(username);
        UserAlbumMeta ua = user.AlbumMeta.Single(x => x.AlbumName == albumName);
        UserAlbumTagMeta? tagMeta = ua.Tags.FirstOrDefault(f => f.TagName == t.TagName);
        if (tagMeta == null)
        {
            UserAlbumTagMeta newTagMeta = new() { TagName = t.TagName, Count = 1 };
            ua.Tags.Add(newTagMeta);
        }
        else
        {
            tagMeta.Count++;
        }

        WriteUser(user);
    }

    public static void IncreaseLikeCount(string username, string albumName, bool isFirstLike)
    {
        UserMeta user = ReadUser(username);
        UserAlbumMeta ua = user.AlbumMeta.Single(x => x.AlbumName == albumName);
        ua.TotalLikes++;
        if (isFirstLike) ua.TotalUniqueLikes++;
        WriteUser(user);
    }

    public static void InitializeAlbumMeta(string username, string albumName)
    {
        UserMeta userMeta = ReadUser(username);
        UserAlbumMeta m = new()
        {
            AlbumName = albumName,
            Created = DateTime.UtcNow,
            Tags = []
        };
        userMeta.AlbumMeta.Add(m);
        WriteUser(userMeta);
    }

    public static void AddSavedSearch(string username, SavedSearchMeta searchMeta)
    {
        UserMeta userMeta = ReadUser(username);

        if (userMeta.SavedSearches == null) userMeta.SavedSearches = [];
        else userMeta.SavedSearches.RemoveAll(s => s.SearchName == searchMeta.SearchName);

        searchMeta.LastUpdated = DateTime.UtcNow;
        userMeta.SavedSearches.Add(searchMeta);
        WriteUser(userMeta);
    }

    // helpers
    static UserMeta ReadUser(string username)
    {
        string path = GetPathUser(username);
        string content = File.ReadAllText(path);
        UserMeta userMeta = JsonSerializer.Deserialize<UserMeta>(content) ?? throw new Exception("User cannot be null here");

        return userMeta;
    }

    public static void WriteUser(UserMeta userMeta)
    {
        string file = GetPathUser(userMeta.Username);
        string json = JsonSerializer.Serialize(userMeta);
        File.WriteAllText(file, json);
    }

    static void UpdateTagMeta(UserAlbumMeta albumMeta, Tag t)
    {
        UserAlbumTagMeta tagMeta = albumMeta.Tags.Single(f => f.TagName == t.TagName);
        if (tagMeta.Count <= 1) albumMeta.Tags.Remove(tagMeta);
        else tagMeta.Count--;
    }

    static string GetPathUser(string username)
    {
        string userDir = Path.Combine(Globals.StoragePath, username);

        return GetFilenameUser(userDir, username);
    }
    
    static string GetFilenameUser(string userDir, string username)
    {
        return Path.Combine(userDir, $"{username}_meta.{Globals.FILE_EXT}");
    }
}