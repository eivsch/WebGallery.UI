using DomainModel.Aggregates.Gallery;
using DomainModel.Aggregates.Gallery.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Infrastructure.Galleries.DTO;
using Infrastructure.Common;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;

namespace Infrastructure.Galleries
{
    public class GalleryRepository : IGalleryRepository
    {
        HttpClient _client;

        public GalleryRepository(WebGalleryApiClient client, IHttpContextAccessor httpContext)
        {
            _client = client?.Client ?? throw new ArgumentNullException(nameof(client));

            //AddUserContextToApiClient();

            void AddUserContextToApiClient()
            {
                var user = httpContext.HttpContext.User;
                var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid).Value;
                if (!string.IsNullOrWhiteSpace(userId))
                    _client.DefaultRequestHeaders.Add("Gallery-User", userId);
            }
        }


        public Task<Gallery> Find(Gallery aggregate)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Gallery>> FindAll(Gallery aggregate)
        {
            throw new NotImplementedException();
        }

        public Task<Gallery> FindById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Gallery> FindById(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Gallery> FindById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Gallery> Get(string id, int itemIndexStart, int numberOfItems)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"galleries/{id}?itemIndexStart={itemIndexStart}&numberOfItems={numberOfItems}");

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<GalleryDTO>(responseStream);

                Gallery gallery = Gallery.Create(data.Id, data.ImageCount, galleryName: data.GalleryName);
                foreach (var item in data.GalleryPictures)
                    gallery.AddGalleryItem(
                        galleryItemId: item.Id, 
                        mediaType: item.MediaType, 
                        appPath: item.AppPath,
                        indexGlobal: item.IndexGlobal
                    );

                return gallery;
            }
            else
            {
                throw new Exception($"The API returned a {response.StatusCode} status code.");
            }
        }

        public async Task<IEnumerable<Gallery>> GetAll()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "galleries");

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<IEnumerable<GalleryDTO>>(responseStream);

                List<Gallery> list = new List<Gallery>();
                foreach(var i in data)
                {
                    list.Add(Gallery.Create(i.Id, i.ImageCount, galleryName: i.GalleryName));
                }

                return list;
            }
            else
            {
                throw new Exception($"The API returned a {response.StatusCode} status code.");
            }
        }

        public async Task<Gallery> GetRandom(string uri)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get, 
                uri);

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<GalleryDTO>(responseStream);

                Gallery gallery = Gallery.Create(data.Id, data.ImageCount);
                foreach (var item in data.GalleryPictures)
                    gallery.AddGalleryItem(
                        galleryItemId: item.Id, 
                        mediaType: item.MediaType, 
                        appPath: item.AppPath,
                        indexGlobal: item.IndexGlobal
                    );

                return gallery;
            }
            else
            {
                throw new Exception($"The API returned a {response.StatusCode} status code.");
            }
        }

        public async Task<string> GetRandomizerUri(int imageCount, string tagList, string tagFilterMode, string mediaFilterMode)
        {
            string uri = $"galleries/customized-random?itemsInEach={imageCount}";
            if (!string.IsNullOrWhiteSpace(tagList))
                uri += $"&tags={tagList}";
            if (!string.IsNullOrWhiteSpace(tagFilterMode))
                uri += $"&tagFilterMode={tagFilterMode}";
            if (!string.IsNullOrWhiteSpace(mediaFilterMode))
                uri += $"&mediaFilterMode={mediaFilterMode}";

            return uri;
        }

        public void Remove(Gallery aggregate)
        {
            throw new NotImplementedException();
        }

        public Task<Gallery> Save(Gallery aggregate)
        {
            throw new NotImplementedException();
        }
    }
}
