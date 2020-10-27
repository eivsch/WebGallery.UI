using System;
using System.Collections.Generic;

namespace WebGallery.UI.ViewModels.Single
{
    public class SingleGalleryViewModel
    {
        public string Id { get; set; }
        public string GalleryType { get; set; }
        public List<SingleGalleryImageViewModel> Images { get; set; } = new List<SingleGalleryImageViewModel>();
    }
}
