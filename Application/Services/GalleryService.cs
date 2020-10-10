using Application.Galleries;
using Application.Services.Interfaces;
using DomainModel.Aggregates.Gallery;
using DomainModel.Aggregates.Gallery.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class GalleryService : IGalleryService
    {
        IGalleryRepository _galleryRepository;

        public GalleryService(IGalleryRepository galleryRepository)
        {
            _galleryRepository = galleryRepository ?? throw new ArgumentNullException(nameof(galleryRepository));
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
                GalleryPictures = response.GalleryItems.Select(s => Map(s))
            };
        }

        public async Task<IEnumerable<GalleryResponse>> GetAll()
        {
            var resp = await _galleryRepository.GetAll();

            var list = new List<GalleryResponse>();
            foreach(var gal in resp)
                list.Add(new GalleryResponse { Id = gal.Id, ImageCount = gal.ImageCount});

            return list;
        }

        private GalleryPicture Map(GalleryItem item)
        {
            return new GalleryPicture
            {
                Id = item.Id,
                Index = item.Index
            };
        }
    }
}
