using Application.Galleries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IGalleryService
    {
        Task<string> GenerateGalleryUri(int imageCount, string tags = "", string tagFilterMode = "", string mediaFilterMode = "");
        Task<GalleryResponse> Get(string galleryUri);
        Task<IEnumerable<GalleryResponse>> GetAllGalleriesWithoutItems();
    }
}
