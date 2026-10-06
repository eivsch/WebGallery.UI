using MinimalGallery.API.Models;
using MinimalGallery.API.Storage;

namespace MinimalGallery.API;

static class RequestHelper
{
    public static UserCredentials? GetUserCredentials(string username)
    {
        UserMeta? data = UserMetaHandler.GetUserMeta(username);
        if (data == null) return null;

        return new UserCredentials
        {
            Username = data.Username,
            Password = data.Password
        };
    }

    public static void CreateNewAlbum(string username, string albumName)
    {
        AlbumIndexHandler.CreateIndex(username, albumName);
        UserMetaHandler.InitializeAlbumMeta(username, albumName);
    }

    public static bool DeleteAlbum(string username, string albumName)
    {
        bool deleted = AlbumIndexHandler.DeleteIndex(username, albumName);
        UserMetaHandler.DeleteAlbumMeta(username, albumName);

        return deleted;
    }

    public static void AddNewMedia(string username, string albumName, NewMediaRequest r)
    {
        var mediaData = new Media
        {
            Id = Guid.NewGuid().ToString(),
            Name = r.Name,
            Size = r.Size,
            Created = DateTimeOffset.Now
        };

        AlbumIndexHandler.AddMedia(username, albumName, mediaData);
    }

    public static Media? DeleteMedia(string username, string albumName, string searchTerm)
    {
        (Media? media, int? i) = AlbumIndexHandler.FindMediaChunk(username, albumName, searchTerm);
        if (media == null || i == null) return null;

        AlbumIndexHandler.DeleteMediaChunk(username, albumName, i.Value);
        UserMetaHandler.HandleMediaDeletion(username, albumName, media);

        return media;
    }

    public static bool AddTag(string username, string albumName, string searchTerm, NewTagRequest r)
    {
        (Media? media, int? i) = AlbumIndexHandler.FindMediaChunk(username, albumName, searchTerm);
        if (media == null || i == null) return false;

        if (media.Tags.Any(a => a.TagName == r.TagName)) return false;
        Tag t = new() { TagName = r.TagName, Created = DateTime.UtcNow };
        media.Tags.Add(t);

        AlbumIndexHandler.WriteMediaChunk(username, albumName, i.Value, media);
        UserMetaHandler.AddTagMeta(username, albumName, t);

        return true;
    }

    public static bool DeleteTag(string username, string albumName, string mediaLocator, string tag)
    {
        (Media? media, int? i) = AlbumIndexHandler.FindMediaChunk(username, albumName, mediaLocator);
        if (media == null || i == null) return false;

        Tag? t = media.Tags.FirstOrDefault(f => f.TagName == tag);
        if (t == null) return false;
        else media.Tags.Remove(t);

        AlbumIndexHandler.WriteMediaChunk(username, albumName, i.Value, media);
        UserMetaHandler.HandleTagDeletion(username, albumName, [t]);

        return true;
    }

    public static bool IncreaseLikedCount(string username, string albumName, string searchTerm)
    {
        (Media? media, int? i) = AlbumIndexHandler.FindMediaChunk(username, albumName, searchTerm);
        if (media == null || i == null) return false;

        bool isFirstLike = media.Likes == 0;
        media.Likes++;
        AlbumIndexHandler.WriteMediaChunk(username, albumName, i.Value, media);

        UserMetaHandler.IncreaseLikeCount(username, albumName, isFirstLike);

        return true;
    }

    public static bool RenameMedia(string username, string albumName, string mediaLocator, string newMediaName)
    {
        (Media? media, int? index) = AlbumIndexHandler.FindMediaChunk(username, albumName, mediaLocator);
        if (media == null || index == null) return false;

        media.Name = newMediaName;
        AlbumIndexHandler.WriteMediaChunk(username, albumName, index.Value, media);

        return true;
    }

    public static List<SearchHit>? Search(string username, SearchRequest request, DateTimeOffset? createdAfter = null, DateTimeOffset? createdBefore = null)
    {
        string[] albumsArray = [];
        if (request.Albums is not null) albumsArray = request.Albums.Split(",");
        string[] tagsArray = [];
        if (request.Tags is not null) tagsArray = request.Tags.Split(',');
        string[] fileExtensionsArray = [];
        if (request.FileExtensions is not null) fileExtensionsArray = request.FileExtensions.Split(',');

        List<SearchHit> hits = [];
        UserMeta? userMetaData = UserMetaHandler.GetUserMeta(username);
        if (userMetaData == null) return null;

        int totalHits = 0;
        List<UserAlbumMeta> albumsToSearch = FilterListOfAlbums(userMetaData.AlbumMeta, albumsArray, tagsArray, request.AllTagsMustMatch);
        foreach (UserAlbumMeta album in albumsToSearch)
        {
            int readSize = 200;
            int from = 0;
            while (true)
            {
                List<Media>? albumItems = AlbumIndexHandler.GetAlbumItems(username, album.AlbumName, from, readSize);
                if (albumItems == null || albumItems.Count == 0) break;

                int mediaAlbumIndex = 0;
                foreach (Media item in albumItems)
                {
                    bool tagsMatch = true;
                    if (tagsArray.Length > 0)
                    {
                        if (request.AllTagsMustMatch)
                        {
                            foreach (string tag in tagsArray)
                            {
                                if (item.Tags.Any(t => t.TagName == tag)) continue;
                                else tagsMatch = false;
                            }
                        }
                        else
                        {
                            tagsMatch = false;
                            foreach (string tag in tagsArray)
                            {
                                if (item.Tags.Any(t => t.TagName == tag))
                                {
                                    tagsMatch = true;
                                    break; // No need to check further if at least one tag matches
                                }
                            }
                        }
                    }

                    bool extensionMatch = fileExtensionsArray.Length == 0 || fileExtensionsArray.Any(ext => item.Name.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
                    bool mediaNameMatch = request.MediaNameContains is null || item.Name.Contains(request.MediaNameContains, StringComparison.OrdinalIgnoreCase) || item.Id.Contains(request.MediaNameContains, StringComparison.OrdinalIgnoreCase);
                    bool createdAfterMatch = createdAfter is null || item.Created.Date > createdAfter.Value.Date;
                    bool createdBeforeMatch = createdBefore is null || item.Created.Date < createdBefore.Value.Date;
                    bool fileSizeMatch = (request.MinFileSize is null && request.MaxFileSize is null) ||
                        (item.Size is not null &&
                        (request.MinFileSize is null || item.Size >= request.MinFileSize) &&
                        (request.MaxFileSize is null || item.Size <= request.MaxFileSize));

                    if (tagsMatch && extensionMatch && mediaNameMatch && createdAfterMatch && createdBeforeMatch && fileSizeMatch)
                    {
                        totalHits++;
                        if (totalHits > (request.HitsToSkip ?? 0))
                        {
                            SearchHit searchHit = new()
                            {
                                AlbumName = album.AlbumName,
                                MediaAlbumIndex = mediaAlbumIndex,
                                MediaItem = item
                            };
                            hits.Add(searchHit);
                        }
                    }

                    mediaAlbumIndex++;
                    if (hits.Count >= request.MaxResults) break;
                }

                from += readSize;
                if (hits.Count >= request.MaxResults) break;
            }

            if (hits.Count >= request.MaxResults) break;
        }

        return hits;
    }

    private static List<UserAlbumMeta> FilterListOfAlbums(List<UserAlbumMeta> allUserAlbums, string[] albumsArray, string[] tagsArray, bool allTagsMustMatch = true)
    {
        List<UserAlbumMeta> albumsToSearch = [];
        if (albumsArray.Length > 0)
        {
            foreach (UserAlbumMeta album in allUserAlbums)
            {
                if (albumsArray.Any(a => album.AlbumName.Equals(a, StringComparison.OrdinalIgnoreCase))) albumsToSearch.Add(album);
            }
        }
        else albumsToSearch = allUserAlbums;    // We won't do any modifications so it's fine to use the same reference

        List<UserAlbumMeta> albumsWithTags = [];
        if (tagsArray.Length > 0)
        {
            foreach (UserAlbumMeta album in albumsToSearch)
            {
                bool tagsMatch = true;
                if (allTagsMustMatch)
                {
                    // All tags must be present in album
                    foreach (string tag in tagsArray)
                    {
                        if (album.Tags.Any(t => t.TagName == tag)) continue;
                        else tagsMatch = false;
                    }
                }
                else
                {
                    // At least one tag must be present in album
                    tagsMatch = false;
                    foreach (string tag in tagsArray)
                    {
                        if (album.Tags.Any(t => t.TagName == tag))
                        {
                            tagsMatch = true;
                            break;
                        }
                    }
                }

                if (tagsMatch) albumsWithTags.Add(album);
            }
        }

        if (albumsWithTags.Count > 0) albumsToSearch = albumsWithTags;

        return albumsToSearch;
    }

    public static UserAlbumMeta MergeAlbums(string username, List<string> sourceAlbums, string targetAlbumName)
    {
        if (sourceAlbums == null || sourceAlbums.Count == 0) throw new ArgumentException("At least one source album must be provided.", nameof(sourceAlbums));

        // Normalize source list and remove any accidental references to the target
        List<string> distinctSources = sourceAlbums.Distinct().ToList();

        // Check if target index file already exists
        string targetPath = Path.Combine(Globals.StoragePath, username, $"{targetAlbumName}.{Globals.FILE_EXT}");
        bool targetExists = File.Exists(targetPath);

        // If target does not exist, rename the first source to target (this creates the target index)
        string renamedSource = null;
        if (!targetExists)
        {
            string firstSource = distinctSources[0];
            AlbumIndexHandler.RenameAlbumIndexFile(username, firstSource, targetAlbumName);
            renamedSource = firstSource;
            // treat the renamed source as contributing to the target (so we still include its meta below)
            distinctSources.Remove(firstSource);
            targetExists = true;
        }
        else
        {
            // If the target exists we should not try to rename any source; remove target from sources if present
            distinctSources.RemoveAll(s => string.Equals(s, targetAlbumName, StringComparison.OrdinalIgnoreCase));
        }

        int itemCountPreMerge = AlbumIndexHandler.GetAlbumItemsCount(username, targetAlbumName);
        int readSize = 10000;

        // Merge remaining source albums into target
        foreach (string albumName in distinctSources.ToList())
        {
            int from = 0;
            while (true)
            {
                List<Media>? items = AlbumIndexHandler.GetAlbumItems(username, albumName, from, readSize);
                if (items == null || items.Count == 0) break;

                foreach (Media media in items)
                {
                    // ensure unique name in target album
                    string originalName = media.Name;
                    int suffix = 1;
                    (Media? existing, int? i) = AlbumIndexHandler.FindMediaChunk(username, targetAlbumName, media.Name, 0, itemCountPreMerge);
                    while (existing is not null)
                    {
                        string namePart = Path.GetFileNameWithoutExtension(originalName);
                        media.Name = namePart + "_" + suffix + Path.GetExtension(originalName);
                        suffix++;
                        (existing, i) = AlbumIndexHandler.FindMediaChunk(username, targetAlbumName, media.Name, 0, itemCountPreMerge);
                    }

                    AlbumIndexHandler.AddMedia(username, targetAlbumName, media);
                    itemCountPreMerge++; // account for newly added item
                }

                from += readSize;
            }

            // After merging, delete the source index file
            AlbumIndexHandler.DeleteIndex(username, albumName);
        }

        // 2. merge the user meta object
        UserMeta? user = UserMetaHandler.GetUserMeta(username) ?? throw new Exception("User does not exist.");

        // collect metas for all source albums and the existing target (if any)
        List<UserAlbumMeta> metasToMerge = new List<UserAlbumMeta>();

        // If we renamed one source into the target, its meta still exists under the original name and should be merged
        foreach (string src in sourceAlbums)
        {
            UserAlbumMeta? m = user.AlbumMeta.FirstOrDefault(s => s.AlbumName == src);
            if (m is not null) metasToMerge.Add(m);
        }

        // also include existing target meta if it existed originally and wasn't part of sourceAlbums
        if (user.AlbumMeta.Any(a => a.AlbumName == targetAlbumName) && !metasToMerge.Any(m => m.AlbumName == targetAlbumName))
        {
            UserAlbumMeta? tmeta = user.AlbumMeta.FirstOrDefault(a => a.AlbumName == targetAlbumName);
            if (tmeta is not null) metasToMerge.Add(tmeta);
        }

        // merge album tags and counts
        List<UserAlbumTagMeta> mergedTags = new List<UserAlbumTagMeta>();
        int totalLikes = 0;
        int totalUniqueLikes = 0;

        foreach (UserAlbumMeta srcMeta in metasToMerge)
        {
            if (srcMeta.Tags != null)
            {
                foreach (UserAlbumTagMeta t in srcMeta.Tags)
                {
                    UserAlbumTagMeta? existingTag = mergedTags.FirstOrDefault(f => f.TagName == t.TagName);
                    if (existingTag is not null)
                    {
                        existingTag.Count += t.Count;
                    }
                    else
                    {
                        mergedTags.Add(new UserAlbumTagMeta { TagName = t.TagName, Count = t.Count });
                    }
                }
            }

            totalLikes += srcMeta.TotalLikes;
            totalUniqueLikes += srcMeta.TotalUniqueLikes;
        }

        int totalCount = AlbumIndexHandler.GetAlbumItemsCount(username, targetAlbumName);

        UserAlbumMeta mergedAlbum = new()
        {
            AlbumName = targetAlbumName,
            Created = DateTime.UtcNow,
            Tags = mergedTags,
            TotalLikes = totalLikes,
            TotalUniqueLikes = totalUniqueLikes,
            TotalCount = totalCount
        };

        // remove original albums (sources and original target if present) and add the merged one
        HashSet<string> namesToRemove = new HashSet<string>(sourceAlbums, StringComparer.OrdinalIgnoreCase);
        // also remove original target meta if present to replace with merged
        namesToRemove.Add(targetAlbumName);

        user.AlbumMeta.RemoveAll(a => namesToRemove.Contains(a.AlbumName));
        user.AlbumMeta.Add(mergedAlbum);
        UserMetaHandler.WriteUser(user);

        return mergedAlbum;
    }

    public static void RebuildAlbumIndex(string username, string albumName, string rebuildType)
    {
        int currentChunkIndex = 0;
        int chunkIterations = AlbumIndexHandler.GetAlbumItemsCount(username, albumName) - 1;    // we have nothing to compare against on the last row, so we skip the last iteration
        while (currentChunkIndex < chunkIterations)
        {
            int from = currentChunkIndex;
            Media currentLowest = AlbumIndexHandler.GetAlbumItems(username, albumName, from, 1)?.First() ?? throw new Exception();
            currentLowest.Index = from;
            while (true)
            {
                from++;
                Media? cmp = AlbumIndexHandler.GetAlbumItems(username, albumName, from, 1)?.FirstOrDefault();
                if (cmp is null) break;
                if (rebuildType == "date")
                {
                    if (DateTimeOffset.Compare(currentLowest.Created, cmp.Created) > 0)
                    {
                        currentLowest = cmp;
                        currentLowest.Index = from;
                    }
                }
                else
                {
                    if (WinApiCalls.StrCmpLogicalW(currentLowest.Name, cmp.Name) > 0) // what do we do if not windows?
                    {
                        currentLowest = cmp;
                        currentLowest.Index = from;
                    }
                }
            }

            if (currentChunkIndex != currentLowest.Index)
            {
                // switch out the current lowest with the one we have at our current position
                Media tmp = AlbumIndexHandler.GetAlbumItems(username, albumName, currentChunkIndex, 1)?.First() ?? throw new Exception();
                AlbumIndexHandler.WriteMediaChunk(username, albumName, currentChunkIndex, currentLowest);
                AlbumIndexHandler.WriteMediaChunk(username, albumName, currentLowest.Index.Value, tmp);
            }

            currentChunkIndex++;
        }
    }

    public static MoveMediaResponse MoveMedia(string username, string sourceAlbum, string mediaLocator, string targetAlbum, string mediaName)
    {
        (Media? existingMedia, _) = AlbumIndexHandler.FindMediaChunk(username, targetAlbum, mediaName);
        if (existingMedia != null) return new MoveMediaResponse(false, "A media with the same name already exists in the target album.");

        Media? m = DeleteMedia(username, sourceAlbum, mediaLocator);
        if (m == null) return new MoveMediaResponse(false, "Media not found in the source album.");
        
        AlbumIndexHandler.AddMedia(username, targetAlbum, m);
        foreach (Tag tag in m.Tags) UserMetaHandler.AddTagMeta(username, targetAlbum, tag);

        return new MoveMediaResponse(true, null);
    }

    public static void RunAdHocJob(string username, string jobName)
    {
        if (string.IsNullOrWhiteSpace(jobName)) throw new ArgumentException("Job name cannot be null or whitespace.", nameof(jobName));
        else if (jobName == "tag-video-images")
        {
            // Loop through all albums with "_video_images" in the name and tag all media items with the tag "screencap".
            UserMeta? user = UserMetaHandler.GetUserMeta(username);
            if (user == null) throw new Exception($"User '{username}' not found.");
            
            var albums = user.AlbumMeta.Where(a => a.AlbumName.Contains("_video_images")).ToList();
            foreach (UserAlbumMeta? album in albums)
            {
                int from = 0;
                int readSize = 100;
                while (true)
                {
                    List<Media>? items = AlbumIndexHandler.GetAlbumItems(username, album.AlbumName, from, readSize);
                    if (items == null || items.Count == 0) break;

                    foreach (Media media in items)
                    {
                        if (!media.Tags.Any(t => t.TagName == "screencap"))
                        {
                            Tag newTag = new() { TagName = "screencap", Created = DateTime.UtcNow };
                            media.Tags.Add(newTag);
                            AlbumIndexHandler.WriteMediaChunk(username, album.AlbumName, media.Index!.Value, media);
                            UserMetaHandler.AddTagMeta(username, album.AlbumName, newTag);
                        }
                    }

                    from += readSize;
                }
            }
        }
        else
        {
            throw new ArgumentException($"Unknown job name: {jobName}", nameof(jobName));
        }
    }
}