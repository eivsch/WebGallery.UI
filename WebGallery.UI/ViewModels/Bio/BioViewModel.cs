using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebGallery.UI.ViewModels.Bio
{
    public class BioViewModel
    {
        public List<string> AllTags { get; set; }
        public BioPictureViewModel BioPictureViewModel { get; set; }
    }
}
