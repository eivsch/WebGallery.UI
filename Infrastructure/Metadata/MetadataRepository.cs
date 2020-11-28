using DomainModel.Aggregates.Metadata;
using DomainModel.Aggregates.Metadata.Interfaces;
using Infrastructure.Common;
using Infrastructure.Metadata.DTO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Metadata
{
    public class MetadataRepository : IMetadataRepository
    {
        HttpClient _client;

        public MetadataRepository(ApiClient client)
        {
            _client = client.Client;
        }

        public async Task<DomainModel.Aggregates.Metadata.Metadata> Get(MetadataType type)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"metadata?type={type}");
            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();

                var data = await JsonSerializer.DeserializeAsync<MetadataDTO>(responseStream);

                var aggregate = DomainModel.Aggregates.Metadata.Metadata.Create(data.Name, type, data.TotalCount, data.Details);

                return aggregate;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            else
            {
                throw new Exception($"The API returned a {response.StatusCode} status code.");
            }
        }
    }
}
