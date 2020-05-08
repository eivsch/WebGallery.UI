namespace WebGallery.UI.ViewModels
{
    public class HomeGalleryViewModel : ImageViewModelBase
    {
        public string Id { get; set; }
        public int CoverImageIndex { get; set; }
        public string Title { get; set; }
        public int ItemCount { get; set; }
    }
}
