using Application.Enums;

namespace WebGallery.UI.ViewModels.Single
{
    public class SingleGalleryImageViewModel : GridImageViewModelBase
    {
        public string Id { get; set; }
        public string AppPath { get; set; }
        public int IndexGlobal { get; set; }
        public int GalleryIndex { get; set; }
        public MediaType MediaType { get; set; }
    }
}
