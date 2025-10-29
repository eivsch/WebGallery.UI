using WebGallery.UI.Models;

namespace WebGallery.UI.ViewModels.Tags
{
    public class TagsViewModel
    {
        public string CategoryName { get; set; }
        public string CoverImageAppPath { get; set; }
        public MediaType CoverImageMediaType { get; set; }
        public int ItemCount { get; set; }
    }
}
