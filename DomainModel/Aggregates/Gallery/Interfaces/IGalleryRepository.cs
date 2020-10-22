using DomainModel.Common.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DomainModel.Aggregates.Gallery.Interfaces
{
    public interface IGalleryRepository : IRepository<Gallery>
    {
        public Task<IEnumerable<Gallery>> GetAll();
        public Task<Gallery> Get(string id, int itemIndexStart, int numberOfItems);
        public Task<string> GetRandomizerUri(int imageCount, string tagList, string tagFilterMode, string mediaFilterMode);
        public Task<Gallery> GetRandom(string uri);

    }
}
