using System.Collections.Generic;
using WebGallery.UI.Models;

namespace WebGallery.UI.ViewModels.Customizer
{
    public class CustomizerViewModel
    {
        public int NumberOfPictures { get; set; }
        public string RadioTagModeOption { get; set; }
        public string RadioTagFilterOption { get; set; }
        public string RadioMediaFilterModeOption { get; set; }
        public List<Tag> Tags { get; set; }
        public List<string> SelectedTags { get; set; }
    }
}
