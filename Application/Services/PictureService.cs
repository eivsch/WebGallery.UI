using Application.Pictures;
using Application.Services.Interfaces;
using AutoMapper;
using DomainModel.Aggregates.Picture.Interfaces;
using System;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PictureService : IPictureService
    {
        private readonly IPictureRepository _pictureRepository;
        private readonly IMapper _mapper;

        public PictureService(IPictureRepository pictureRepository, IMapper mapper)
        {
            _pictureRepository = pictureRepository;
            _mapper = mapper;
        }

        public async Task<PictureResponse> Get(string id)
        {
            var aggregate = await _pictureRepository.FindById(id);

            return _mapper.Map<PictureResponse>(aggregate);        
        }

        public async Task<PictureResponse> Get(int index)
        {
            var aggregate = await _pictureRepository.FindById(index);

            return _mapper.Map<PictureResponse>(aggregate);
        }

        public async Task<PictureResponse> Get(string galleryId, int index)
        {
            var aggregate = await _pictureRepository.GetPicture(galleryId, index);

            return _mapper.Map<PictureResponse>(aggregate);
        }
    }
}
