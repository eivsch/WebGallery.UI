using Application.Metadata;
using Application.Services.Interfaces;
using DomainModel.Aggregates.Metadata;
using DomainModel.Aggregates.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class MetadataService : IMetadataService
    {
        private readonly IMetadataRepository _metadataRepository;

        public MetadataService(IMetadataRepository metadataRepository)
        {
            _metadataRepository = metadataRepository;
        }

        public async Task<MetadataResponse> GetStatistics(string itemType)
        {
            Enum.TryParse(itemType, out MetadataType type);
            var aggregate = await _metadataRepository.Get(type);

            return new MetadataResponse
            {
                ShortDescription = aggregate.ShortDescription,
                InfoItems = aggregate.Details.InfoItems
            };
        }
    }
}
