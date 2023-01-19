using DomainModel.Aggregates.Picture;
using DomainModel.Aggregates.Picture.Interfaces;
using Infrastructure.Common;
using Infrastructure.Pictures.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Pictures
{
    public class PictureRepository : IPictureRepository
    {
        HttpClient _client;

        public PictureRepository(WebGalleryApiClient client)
        {
            _client = client?.Client ?? throw new ArgumentNullException(nameof(client));
        }

        public Task<Picture> Find(Picture aggregate)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Picture>> FindAll(Picture aggregate)
        {
            throw new NotImplementedException();
        }

        public Task<Picture> FindById(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<Picture> FindById(string id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"pictures/sha/{id}");

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<PictureDTO>(responseStream);

                return Map(data);
            }
            else
            {
                throw new Exception($"The API returned a {response.StatusCode} status code.");
            }
        }

        public async Task<Picture> FindById(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"pictures/{id}");

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<PictureDTO>(responseStream);

                return Map(data);
            }
            else
            {
                throw new Exception($"The API returned a {response.StatusCode} status code.");
            }
        }

        private Picture Map(PictureDTO dto)
        {
            var aggregate = Picture.Create(
                id: dto.Id,
                name: dto.Name,
                appPath: dto.AppPath,
                originalPath: dto.OriginalPath,
                folderName: dto.FolderName,
                folderId: dto.FolderId,
                globalSortOrder: dto.GlobalSortOrder,
                folderSortOrder: dto.FolderSortOrder,
                size: dto.Size,
                created: dto.CreateTimestamp
            );

            dto.Tags?.ToList().ForEach(tag => aggregate.AddTag(tag));

            return aggregate;
        }

        public void Remove(Picture aggregate)
        {
            throw new NotImplementedException();
        }

        public async Task<Picture> Save(Picture aggregate)
        {
            var jsonOpts = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var jsonContent = new JsonContent(aggregate, jsonOpts);

            var response = await _client.PostAsync("pictures", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<PictureDTO>(responseStream);

                return Map(data);
            }
            else
            {
                throw new Exception($"The API returned a {response.StatusCode} status code.");
            }
        }

        public async Task<Picture> GetRandomPicture(string galleryId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"pictures/random?albumId={galleryId}");

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<PictureDTO>(responseStream);

                return Map(data);
            }
            else
            {
                throw new Exception($"The API returned a {response.StatusCode} status code.");
            }
        }

        public async Task<List<Picture>> SearchPictures(string query)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"pictures/search?query={query}");

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<IEnumerable<PictureDTO>>(responseStream);

                var result = data.Select(s => Map(s));

                return result.ToList();
            }
            else
            {
                throw new Exception($"The API returned a {response.StatusCode} status code.");
            }
        }

        public async Task Remove(string id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"pictures/sha/{id}");
            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"The API returned a {response.StatusCode} status code.");
            }
        }
    }
}
