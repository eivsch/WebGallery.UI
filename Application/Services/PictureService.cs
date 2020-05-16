using Application.Pictures;
using Application.Services.Interfaces;
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

        public PictureService(IPictureRepository pictureRepository)
        {
            _pictureRepository = pictureRepository;
        }

        public async Task<PictureResponse> Get(int index)
        {
            var picture = await _pictureRepository.FindById(index);

            return Map(picture);
        }

        public async Task<IEnumerable<PictureResponse>> GetPictures(string galleryId, int offset = 0)
        {
            var pictures = await _pictureRepository.GetPictures(galleryId, offset);

            var list = new List<PictureResponse>();
            foreach (var picture in pictures)
            {
                list.Add(Map(picture));
            }

            return list;
        }

        private PictureResponse Map(Picture aggregate)
        {
            return new PictureResponse
            {
                Id = aggregate.Id,
                AppPath = aggregate.AppPath,
                CreateTimestamp = aggregate.CreateTimestamp,
                FolderId = aggregate.FolderId,
                FolderName = aggregate.FolderName,
                FolderSortOrder = aggregate.FolderSortOrder,
                GlobalSortOrder = aggregate.GlobalSortOrder,
                Name = aggregate.Name,
                OriginalPath = aggregate.OriginalPath,
                Size = aggregate.Size,
                Tags = aggregate.Tags
            };
        }
    }
}
