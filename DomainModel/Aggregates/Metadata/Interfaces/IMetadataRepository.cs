using System.Threading.Tasks;

namespace DomainModel.Aggregates.Metadata.Interfaces
{
    public interface IMetadataRepository
    {
        Task<Metadata> Get(MetadataType type);
        int GetMaxGlobalIndex();
    }
}
