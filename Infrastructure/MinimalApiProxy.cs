using System;
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
}

public class CredentialsDTO
{
    public string Username {get;set;}
    public string Password {get;set;}
}
