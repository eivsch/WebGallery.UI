using DomainModel.Aggregates.Tags;
using DomainModel.Aggregates.Tags.Interfaces;
using Infrastructure.Common;
using Infrastructure.Tags.DTO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure.Tags
{
    public class TagRepository : ITagRepository
    {
        HttpClient _client;

        public TagRepository(ApiClient client)
        {
            _client = client.Client;
        }

        public Task<Tag> Find(Tag aggregate)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Tag>> FindAll(Tag aggregate)
        {
            throw new NotImplementedException();
        }

        public Task<Tag> FindById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Tag> FindById(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Tag> FindById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<string>> GetAll()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "tags");

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<IEnumerable<string>>(responseStream);

                return data;
            }
            else
            {
                throw new Exception($"The API returned a {response.StatusCode} status code.");
            }
        }

        public void Remove(Tag aggregate)
        {
            throw new NotImplementedException();
        }

        public async Task<Tag> Save(Tag aggregate)
        {
            var dto = new TagDTO
            {
                PictureId = aggregate.PictureId,
                PictureIndex = aggregate.PictureIndex ?? 0,
                Tag = aggregate.TagName
            };

            var response = await _client.PostAsync("tags", new JsonContent(dto));

            if (response.IsSuccessStatusCode)
            {
                return aggregate;
            }
            else
            {
                throw new Exception($"The API returned a {response.StatusCode} status code.");
            }
        }
    }
}
