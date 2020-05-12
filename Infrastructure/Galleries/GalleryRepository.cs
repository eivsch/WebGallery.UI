using DomainModel.Aggregates.Gallery;
using DomainModel.Aggregates.Gallery.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Infrastructure.Galleries.DTO;
using Infrastructure.Common;

namespace Infrastructure.Galleries
{
    public class GalleryRepository : IGalleryRepository
    {
        HttpClient _client;

        public GalleryRepository(ApiClient client)
        {
            _client = client?.Client ?? throw new ArgumentNullException(nameof(client));
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
                    list.Add(Gallery.Create(i.Id, i.ImageCount));
                }

                return list;
            }
            else
            {
                throw new Exception($"The API returned a {response.StatusCode} status code.");
            }
        }

        public async Task<Gallery> GetRandom(int itemsInEach)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"galleries/random?num=1&itemsInEach={itemsInEach}");

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<GalleryDTO>(responseStream);

                Gallery gallery = Gallery.Create(data.Id, data.ImageCount);
                foreach(var item in data.GalleryPictures)
                    gallery.AddGalleryItem(galleryItemId: item.Id, index: item.Index);

                return gallery;
            }
            else
            {
                throw new Exception($"The API returned a {response.StatusCode} status code.");
            }
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
