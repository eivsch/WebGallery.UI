using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Infrastructure.Common;
using Infrastructure.FileServer;

namespace Infrastructure.MinimalApi;

public class MinimalApiProxy(WebGalleryApiClient client)
{
    readonly HttpClient _client = client?.Client;
    readonly JsonSerializerOptions _jsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<CredentialsDTO> GetCredentials(string username)
    {
        HttpResponseMessage response = await _client.GetAsync($"/users/{username}");
        if (response.IsSuccessStatusCode)
        {
            if (response.Content == null || response.Content.Headers.ContentLength == 0)
            {
                return null;
            }

            string responseStr = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseStr))
            {
                return null;
            }

            CredentialsDTO data = JsonSerializer.Deserialize<CredentialsDTO>(responseStr, _jsonOpts);

            return data;
        }
        else
        {
            throw new Exception($"The API returned a {response.StatusCode} status code.");
        }
    }

    public async Task PostUser(string username, string password)
    {
        var body = new
        {
            username = username,
            password = password
        };

        var jsonContent = new JsonContent(body);
        var response = await _client.PostAsync("users", jsonContent);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"The API returned a {response.StatusCode} status code.");
        }
    }

    public async Task<List<AlbumMetaDTO>> GetAlbums(string username)
    {
        HttpResponseMessage response = await _client.GetAsync($"/users/{username}/albums");
        if (response.IsSuccessStatusCode)
        {
            string responseStr = await response.Content.ReadAsStringAsync();
            List<AlbumMetaDTO> data = JsonSerializer.Deserialize<List<AlbumMetaDTO>>(responseStr, _jsonOpts);

            return data;
        }
        else
        {
            throw new Exception($"The API returned a {response.StatusCode} status code.");
        }
    }

    public async Task CreateAlbum(string username, string albumName)
    {
        var body = new
        {
            AlbumName = albumName,
        };

        var jsonContent = new JsonContent(body);
        var response = await _client.PostAsync($"users/{username}/albums", jsonContent);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"The API returned a {response.StatusCode} status code.");
        }
    }

    public async Task PostMediaItem(string username, string albumName, SavedFileInfo savedFileInfo)
    {
        var body = new
        {
            Name = savedFileInfo.FileName,
            Size = savedFileInfo.FileSize
        };

        var jsonContent = new JsonContent(body);
        var response = await _client.PostAsync($"users/{username}/albums/{albumName}/media-items", jsonContent);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"The API returned a {response.StatusCode} status code.");
        }
    }

    public async Task<AlbumContentsDTO> GetAlbumContents(string username, string albumName, int from, int numberOfItems)
    {
        HttpResponseMessage response = await _client.GetAsync($"/users/{username}/albums/{albumName}?from={from}&size={numberOfItems}");
        if (response.IsSuccessStatusCode)
        {
            string responseStr = await response.Content.ReadAsStringAsync();
            AlbumContentsDTO data = JsonSerializer.Deserialize<AlbumContentsDTO>(responseStr, _jsonOpts);

            return data;
        }
        else
        {
            throw new Exception($"The API returned a {response.StatusCode} status code.");
        }
    }

    public async Task PostTag(string username, string albumName, string mediaLocator, string tag)
    {
        var body = new
        {
            TagName = tag,
        };

        var jsonContent = new JsonContent(body);
        var response = await _client.PostAsync($"users/{username}/albums/{albumName}/{mediaLocator}/tags", jsonContent);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"The API returned a {response.StatusCode} status code.");
        }
    }

    public async Task<bool> DeleteTag(string username, string albumName, string mediaLocator, string tag)
    {
        HttpResponseMessage response = await _client.DeleteAsync($"users/{username}/albums/{albumName}/{mediaLocator}/tags/{tag}");
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return true;
        else return false;
    }

    public async Task PatchAddLike(string username, string album, string mediaLocator)
    {
        HttpResponseMessage response = await _client.PatchAsync($"users/{username}/albums/{album}/{mediaLocator}/likes", null);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"The API returned a {response.StatusCode} status code.");
        }
    }

    public async Task<bool> DeleteMedia(string username, string albumName, string mediaLocator)
    {
        HttpResponseMessage response = await _client.DeleteAsync($"users/{username}/albums/{albumName}/{mediaLocator}");
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return true;
        else return false;
    }

    public async Task<bool> DeleteAlbum(string username, string albumName)
    {
        HttpResponseMessage response = await _client.DeleteAsync($"users/{username}/albums/{albumName}");
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return true;
        else return false;
    }

    public async Task<List<SearchHitDTO>> GetSearch(string username, string albums, string tags, string fileExtension, string mediaNameContains, int? maxSize, bool allTagsMustMatch, int? hitsToSkip = null)
    {
        string uri = $"users/{username}/search";
        Dictionary<string, string> paramss = [];
        if (albums is not null)
            paramss.Add("albums", albums);
        if (tags is not null)
            paramss.Add("tags", tags);
        if (fileExtension is not null)
            paramss.Add("fileExtensions", fileExtension);
        if (mediaNameContains is not null)
            paramss.Add("mediaNameContains", mediaNameContains);
        if (maxSize.HasValue)
            paramss.Add("maxSize", maxSize.ToString());
        if (hitsToSkip.HasValue)
            paramss.Add("hitsToSkip", hitsToSkip.Value.ToString());

        paramss.Add("allTagsMustMatch", allTagsMustMatch.ToString().ToLowerInvariant());

        bool isFirstParam = true;
        foreach (KeyValuePair<string, string> param in paramss)
        {
            if (isFirstParam) uri += $"?{param.Key}={param.Value}";
            else uri += $"&{param.Key}={param.Value}";

            isFirstParam = false;
        }

        HttpResponseMessage response = await _client.GetAsync(uri);
        if (response.IsSuccessStatusCode)
        {
            string responseStr = await response.Content.ReadAsStringAsync();
            List<SearchHitDTO> data = JsonSerializer.Deserialize<List<SearchHitDTO>>(responseStr, _jsonOpts);

            return data;
        }
        else
        {
            throw new Exception($"The API returned a {response.StatusCode} status code.");
        }
    }

    public async Task SaveSearch(string username, SavedSearchDTO search)
    {
        var jsonContent = new JsonContent(search);
        var response = await _client.PostAsync($"/users/{username}/saved-searches", jsonContent);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"The API returned a {response.StatusCode} status code.");
        }
    }

    public async Task<List<SavedSearchDTO>> GetSavedSearches(string username)
    {
        var response = await _client.GetAsync($"/users/{username}/saved-searches");
        if (response.IsSuccessStatusCode)
        {
            string responseStr = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<List<SavedSearchDTO>>(responseStr, _jsonOpts);
            return data;
        }
        else
        {
            throw new Exception($"The API returned a {response.StatusCode} status code.");
        }
    }

    public async Task DeleteSavedSearch(string username, string searchName)
    {
        var response = await _client.DeleteAsync($"/users/{username}/saved-searches/{searchName}");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"The API returned a {response.StatusCode} status code.");
        }
    }
}

public record CredentialsDTO
{
    public string Username {get;set;}
    public string Password {get;set;}
}

public record AlbumMetaDTO
{
    public string AlbumName {get;set;}
    public DateTime Created {get;set;}
    public List<TagMetaDTO> Tags {get;set;}
    public int TotalCount {get;set;}
    public int TotalLikes { get;set;}
    public int TotalUniqueLikes { get;set;}
}

public record TagMetaDTO
{
    public string TagName {get;set;}
    public int Count {get;set;}
}

public record AlbumContentsDTO
{
    public int TotalCount {get;set;}
    public List<MediaDTO> Items {get;set;}
}

public record MediaDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<TagDTO> Tags { get; set; }
    public string AlbumName { get; set; }
    public int? Index { get; set; }
    public int? Size { get; set; }
    public DateTimeOffset Created {get;set;}
}

public record TagDTO
{
    public string TagName {get;set;}
    public DateTimeOffset Created {get;set;}
}

public record SearchHitDTO
{
    public int MediaAlbumIndex { get; set; }
    public required string AlbumName { get; set; }
    public required MediaDTO MediaItem { get; set; }
}

public record SavedSearchDTO
{
    public string SearchName { get; set; }
    public string Albums { get; set; }
    public string Tags { get; set; }
    public string FileExtensions { get; set; }
    public string MediaNameContains { get; set; }
    public int? MaxSize { get; set; }
    public bool? AllTagsMustMatch { get; set; }
}