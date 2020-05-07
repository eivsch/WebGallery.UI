using DomainModel.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Aggregates.Picture.Interfaces
{
    public interface IPictureRepository : IRepository<Picture>
    {
        Task<IEnumerable<Picture>> GetPictures(string galleryId, int offset = 0);
    }
}
