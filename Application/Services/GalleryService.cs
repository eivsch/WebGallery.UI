using Application.Enums;
using Application.Galleries;
using Application.Services.Interfaces;
using AutoMapper;
using DomainModel.Aggregates.Gallery;
using DomainModel.Aggregates.Gallery.Interfaces;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class GalleryService : IGalleryService
    {
        IGalleryRepository _galleryRepository;
        IMapper _mapper;

        public GalleryService(IGalleryRepository galleryRepository, IMapper mapper)
        {
            _galleryRepository = galleryRepository ?? throw new ArgumentNullException(nameof(galleryRepository));
            _mapper = mapper;
        }

        public Task<string> GenerateGalleryUri(int imageCount, string tags = "", string tagFilterMode = "", string mediaFilterMode = "")
        {
            return _galleryRepository.GetRandomizerUri(imageCount, tags, tagFilterMode, mediaFilterMode);
        }

        public async Task<GalleryResponse> Get(string galleryUri)
        {
            var response = await _galleryRepository.GetRandom(galleryUri);

            return new GalleryResponse
            {
                Id = response.Id,
                ImageCount = response.ImageCount,
                GalleryItems = response.GalleryItems.Select(s => Map(s))
            };
        }

        public async Task<GalleryResponse> Get(string galleryId, int itemIndexStart, int numberOfItems)
        {
            var aggregate = await _galleryRepository.Get(galleryId, itemIndexStart, numberOfItems);

            return new GalleryResponse
            {
                Id = aggregate.Id,
                ImageCount = aggregate.ImageCount,
                GalleryItems = aggregate.GalleryItems.Select(s => Map(s))
            };
        }

        public async Task<IEnumerable<GalleryResponse>> GetAllGalleriesWithoutItems()
        {
            var galleriesResponse = await _galleryRepository.GetAll();

            var list = new List<GalleryResponse>();
            foreach(var aggregate in galleriesResponse)
                list.Add(_mapper.Map<GalleryResponse>(aggregate));

            return list;
        }

        private Galleries.GalleryItem Map(DomainModel.Aggregates.Gallery.GalleryItem item)
        {
            return new Galleries.GalleryItem
            {
                Id = item.Id,
                Index = item.Index,
                MediaType = Parse(item.MediaType),
            };
        }

        private MediaType Parse(DomainModel.Common.Enums.MediaType mediaType)
        {
            switch (mediaType)
            {
                case DomainModel.Common.Enums.MediaType.Gif:
                    return MediaType.Gif;
                case DomainModel.Common.Enums.MediaType.Image:
                    return MediaType.Image;
                case DomainModel.Common.Enums.MediaType.Video:
                    return MediaType.Video;
                default:
                    return MediaType.Image;
            }
        }
    }
}
