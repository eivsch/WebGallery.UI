using DomainModel.Common.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DomainModel.Aggregates.Tags.Interfaces
{
    public interface ITagRepository : IRepository<Tag>
    {
        Task<IEnumerable<Tag>> GetAll();
    }
}
