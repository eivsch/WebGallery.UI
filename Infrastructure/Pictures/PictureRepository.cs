using DomainModel.Aggregates.Picture;
using DomainModel.Aggregates.Picture.Interfaces;
using Infrastructure.Common;
using Infrastructure.Pictures.DTO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Pictures
{
    public class PictureRepository : IPictureRepository
    {
        HttpClient _client;

        public PictureRepository(ApiClient client)
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

        public Task<Picture> FindById(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Picture> FindById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Picture>> GetPictures(string galleryId, int offset = 0)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"pictures?galleryId={galleryId}&offset={offset}");
            
            var response = await _client.SendAsync(request);
         
            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<IEnumerable<PictureDTO>>(responseStream);

                List<Picture> list = new List<Picture>();
                foreach (var p in data)
                {
                    list.Add(Picture.Create(p.Id, p.GlobalSortOrder, p.FolderSortOrder));
                }

                return list;
            }
            else
            {
                throw new Exception($"The API returned a {response.StatusCode} status code.");
            }
        }

        public void Remove(Picture aggregate)
        {
            throw new NotImplementedException();
        }

        public Task<Picture> Save(Picture aggregate)
        {
            throw new NotImplementedException();
        }
    }
}
