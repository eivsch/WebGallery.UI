using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebGallery.UI.ViewModels
{
    public class PictureViewModel
    {
        public int Id { get; set; }
        public string URL { get; set; }
        public List<ThumbnailViewModel> SubPics { get; set; }
    }
}
