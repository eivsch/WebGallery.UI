using Application.Tags;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface ITagService
    {
        public Task Add(TagRequest request);
    }
}
