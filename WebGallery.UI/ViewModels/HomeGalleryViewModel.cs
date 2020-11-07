using Application.Enums;

namespace WebGallery.UI.ViewModels
{
    public class HomeGalleryViewModel : ImageViewModelBase
    {
        public string GalleryId { get; set; }
        public int CoverImageIndex { get; set; }
        public string CoverImageId { get; set; }
        public MediaType CoverImageMediaType { get; set; }
        public string Title { get; set; }
        public int ItemCount { get; set; }
    }
}
