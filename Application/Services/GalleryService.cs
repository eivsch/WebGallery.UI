using Application.Galleries;
using Application.Services.Interfaces;
using DomainModel.Aggregates.Gallery.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
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

        public Task<GalleryResponse> Get(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<GalleryResponse>> GetAll()
        {
            var resp = await _galleryRepository.GetAll();

            var list = new List<GalleryResponse>();
            foreach(var gal in resp)
                list.Add(new GalleryResponse { Id = gal.Id, ImageCount = gal.ImageCount});

            return list;
        }
    }
}
