using Application.Pictures;
using Application.Services.Interfaces;
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

        public PictureService(IPictureRepository pictureRepository)
        {
            _pictureRepository = pictureRepository;
        }

        public async Task<IEnumerable<PictureResponse>> GetPictures(string galleryId, int offset = 0)
        {
            var pics = await _pictureRepository.GetPictures(galleryId, offset);

            var list = new List<PictureResponse>();
            foreach(var pic in pics)
                list.Add(new PictureResponse { Id = pic.Id, GlobalSortOrder = pic.GlobalSortOrder, FolderSortOrder = pic.FolderSortOrder });

            return list;
        }
    }
}
