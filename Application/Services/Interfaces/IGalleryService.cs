using Application.Galleries;
using Application.Pictures;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IGalleryService
    {
        Task<GalleryResponse> Get(string galleryId, int itemIndexStart, int numberOfItems);
        Task<string> GenerateGalleryUri(int imageCount, string tags = "", string tagFilterMode = "", string mediaFilterMode = "");
        Task<GalleryResponse> Get(string galleryUri);
        Task<IEnumerable<GalleryResponse>> GetAllGalleriesWithoutItems();
        Task<GalleryResponse> ConvertFromPictureList(List<PictureResponse> pictures);
    }
}
