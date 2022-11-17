using Application.Galleries;
using Application.Pictures;
using Application.Services.Interfaces;
using AutoMapper;
using DomainModel.Aggregates.Gallery.Interfaces;
using System;
using System.Collections.Generic;
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

        public Task<GalleryResponse> ConvertFromPictureList(List<PictureResponse> pictures)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateGalleryUri(int imageCount, string tags = "", string tagFilterMode = "", string mediaFilterMode = "")
        {
            return _galleryRepository.GetRandomizerUri(imageCount, tags, tagFilterMode, mediaFilterMode);
        }

        public async Task<GalleryResponse> Get(string galleryUri)
        {
            var aggregate = await _galleryRepository.GetRandom(galleryUri);
            var response = _mapper.Map<GalleryResponse>(aggregate);

            return response;
        }

        public async Task<GalleryResponse> Get(string galleryId, int itemIndexStart, int numberOfItems)
        {
            var aggregate = await _galleryRepository.Get(galleryId, itemIndexStart, numberOfItems);
            var response = _mapper.Map<GalleryResponse>(aggregate);

            return response;
        }

        public async Task<IEnumerable<GalleryResponse>> GetAllGalleriesWithoutItems()
        {
            var galleriesResponse = await _galleryRepository.GetAll();

            var list = new List<GalleryResponse>();
            foreach(var aggregate in galleriesResponse)
                list.Add(_mapper.Map<GalleryResponse>(aggregate));

            return list;
        }
    }
}
