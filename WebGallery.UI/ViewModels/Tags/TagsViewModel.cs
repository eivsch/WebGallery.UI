using Application.Enums;

namespace WebGallery.UI.ViewModels.Tags
{
    public class TagsViewModel
    {
        public string CategoryName { get; set; }
        public string CoverImageId { get; set; }
        public MediaType CoverImageMediaType { get; set; }
        public int ItemCount { get; set; }
    }
}
