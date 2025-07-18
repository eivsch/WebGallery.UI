using System;
using System.Collections.Generic;

namespace WebGallery.UI.ViewModels.Single
{
    public class SingleGalleryViewModel
    {
        public string Id { get; set; }
        public string GalleryTitle { get; set; }
        public List<SingleGalleryImageViewModel> Images { get; set; } = new List<SingleGalleryImageViewModel>();
        public int TotalImageCount { get; set; }
        public int CurrentOffset { get; set; }
        public int DisplayCount { get; set; }
        public bool IsRandomized { get; set; }
    }
}
