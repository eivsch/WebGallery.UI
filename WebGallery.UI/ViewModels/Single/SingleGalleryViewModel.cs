using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebGallery.UI.ViewModels.Single
{
    public class SingleGalleryViewModel
    {
        public string Id { get; set; }
        public List<SingleGalleryImageViewModel> Images { get; set; } = new List<SingleGalleryImageViewModel>();
    }
}
