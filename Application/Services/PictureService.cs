using Application.Pictures;
using Application.Services.Interfaces;
using AutoMapper;
using DomainModel.Aggregates.Picture;
using DomainModel.Aggregates.Picture.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
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

        public async Task<IEnumerable<PictureResponse>> GetPictures(string galleryId, int offset = 0)
        {
            var pictures = await _pictureRepository.GetPictures(galleryId, offset);

            var list = new List<PictureResponse>();
            foreach (var aggregate in pictures)
            {
                list.Add(_mapper.Map<PictureResponse>(aggregate));
            }

            return list;
        }
    }
}
