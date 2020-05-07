using System.Collections.Generic;

namespace WebGallery.UI.ViewModels
{
    public class HomeGalleryViewModel
    {
        public string Id { get; set; }
        public string CoverImageUrl { get; set; }
        public int CoverImageIndex { get; set; }
        public string Title { get; set; }
        public int ItemCount { get; set; }
        public int LargeScreenSize { get; set; }
        public int PopUpDelay { get; set; }
    }
}
