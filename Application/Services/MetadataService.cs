using Application.Metadata;
using Application.Services.Interfaces;
using DomainModel.Aggregates.Metadata;
using DomainModel.Aggregates.Metadata.Interfaces;
using System;
using System.Linq;
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
            MetadataType type = itemType.ToLower() switch
            {
                "album" => MetadataType.Album,
                "gif" => MetadataType.Gif,
                "picture" => MetadataType.Picture,
                "video" => MetadataType.Video,
                "tag" => MetadataType.Tag,
                _ => throw new ArgumentException()
            };

            var aggregate = await _metadataRepository.Get(type);

            return new MetadataResponse
            {
                ShortDescription = aggregate.ShortDescription,
                InfoItems = aggregate.MetadataDetails.InfoItems.ToList()
            };
        }
    }
}
