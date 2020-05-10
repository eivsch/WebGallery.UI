using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebGallery.UI.ViewModels.Bio
{
    public class BioPictureViewModel
    {
        public string PictureId { get; set; }
        public int GlobalSortOrder { get; set; }
        public string Name { get; set; }
        public List<string> Tags { get; set; }
    }
}
