using Application.Metadata;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IMetadataService
    {
        Task<MetadataResponse> GetStatistics(string itemType);
    }
}
