using DomainModel.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Aggregates.Gallery.Interfaces
{
    public interface IGalleryRepository : IRepository<Gallery>
    {
        public Task<IEnumerable<Gallery>> GetAll();

        public Task<Gallery> GetRandom(int numOfPics);

    }
}
