using DomainModel.Common.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DomainModel.Aggregates.Picture.Interfaces
{
    public interface IPictureRepository : IRepository<Picture>
    {
        Task<Picture> GetRandomPicture(string galleryId);
        Task<List<Picture>> SearchPictures(string query);
        Task Remove(string id);
    }
}
