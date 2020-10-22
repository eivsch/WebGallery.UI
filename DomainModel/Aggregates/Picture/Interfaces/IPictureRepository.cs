using DomainModel.Common.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DomainModel.Aggregates.Picture.Interfaces
{
    public interface IPictureRepository : IRepository<Picture>
    {
        Task<IEnumerable<Picture>> GetPictures(string galleryId, int offset = 0);
        Task<Picture> GetPicture(string galleryId, int index = 0);
    }
}
