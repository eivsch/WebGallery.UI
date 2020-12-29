using System.Collections.Generic;

namespace WebGallery.UI.ViewModels.Bio
{
    public class BioPictureViewModel
    {
        public string Id { get; set; }
        public string AppPath { get; set; }
        public int GlobalSortOrder { get; set; }
        public string Name { get; set; }
        public List<string> Tags { get; set; }
    }
}
