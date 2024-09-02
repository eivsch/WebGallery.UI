using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Infrastructure.Common;

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
}

public class CredentialsDTO
{
    public string Username {get;set;}
    public string Password {get;set;}
}

public class AlbumMetaDTO
{
    public string AlbumName {get;set;}
}