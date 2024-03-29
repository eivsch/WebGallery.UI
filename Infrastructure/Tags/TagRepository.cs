﻿using DomainModel.Aggregates.Tags;
using DomainModel.Aggregates.Tags.Interfaces;
using Infrastructure.Common;
using Infrastructure.Tags.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public TagRepository(WebGalleryApiClient client)
        {
            _client = client.Client;
        }

        public async Task DeleteTag(string pictureId, string tagName)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"tags/{pictureId}/{tagName}");

            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"The API returned a {response.StatusCode} status code.");
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

        public async Task<IEnumerable<Tag>> GetAll()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "tags");

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<IEnumerable<TagDTO>>(responseStream);

                var allTags = new List<Tag>();
                foreach (var tag in data)
                {
                    var aggregate = Tag.Create(tag.Name, itemCount: tag.ItemCount);
                    allTags.Add(aggregate);
                }

                return allTags;
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
                Name = aggregate.TagName,
                MediaItems = aggregate.MediaItems.Select(s => Map(s)),
                ItemCount = aggregate.ItemCount
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

        private TagItemDTO Map(TagMediaItem tagMediaItem)
        {
            return new TagItemDTO
            {
                Id = tagMediaItem.Id,
                GlobalIndex = tagMediaItem.GlobalIndex,
                Created = tagMediaItem.Created
            };
        }
    }
}
