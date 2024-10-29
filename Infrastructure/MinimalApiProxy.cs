using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Infrastructure.Common;
using Infrastructure.Services;

namespace Infrastructure.MinimalApi;

public class MinimalApiProxy(WebGalleryApiClient client)
{
    readonly HttpClient _client = client?.Client;
    readonly JsonSerializerOptions _jsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<CredentialsDTO> GetCredentials(string username)
    {
        HttpResponseMessage response =  await _client.GetAsync($"/users/{username}");
        if (response.IsSuccessStatusCode)
        {
            string responseStr = await response.Content.ReadAsStringAsync();
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
        HttpResponseMessage response =  await _client.GetAsync($"/users/{username}/albums");
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
        HttpResponseMessage response =  await _client.GetAsync($"/users/{username}/albums/{albumName}?from={from}&size={numberOfItems}");
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
        var response = await _client.DeleteAsync($"users/{username}/albums/{albumName}/{mediaLocator}/tags/{tag}");
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return true;
        else return false;
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
    public string Id {get;set;}
    public string Name {get;set;}
    public List<TagDTO> Tags {get;set;}
}

public record TagDTO
{
    public string TagName {get;set;}
    public DateTimeOffset Created {get;set;}
}