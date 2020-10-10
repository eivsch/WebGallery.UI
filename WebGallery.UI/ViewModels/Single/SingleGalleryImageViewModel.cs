using Application.Enums;

namespace WebGallery.UI.ViewModels.Single
{
    public class SingleGalleryImageViewModel : ImageViewModelBase
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public int GalleryIndex { get; set; }
        public MediaType MediaType { get; set; }
    }
}
