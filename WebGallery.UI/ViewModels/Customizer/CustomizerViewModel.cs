using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebGallery.UI.Models;

namespace WebGallery.UI.ViewModels.Customizer
{
    public class CustomizerViewModel
    {
        public List<Tag> Tags => new List<Tag>
        {
            new Tag{Name = "Tag1", ThreeLetterIsoCode = "ABC"},
            new Tag{Name = "Tag2", ThreeLetterIsoCode = "DEF"},
            new Tag{Name = "Tag3", ThreeLetterIsoCode = "GHI"},
            new Tag{Name = "Tag4", ThreeLetterIsoCode = "JKL"},
            new Tag{Name = "Tag5", ThreeLetterIsoCode = "MNO"},
            new Tag{Name = "Tag6", ThreeLetterIsoCode = "PQR"},
            new Tag{Name = "Tag7", ThreeLetterIsoCode = "STU"},
            new Tag{Name = "Tag8", ThreeLetterIsoCode = "VWX"},
            new Tag{Name = "Tag9", ThreeLetterIsoCode = "YZZ"},
            new Tag{Name = "Tag10", ThreeLetterIsoCode = "ZZ2"},
            new Tag{Name = "Tag11", ThreeLetterIsoCode = "ZZ3"},
            new Tag{Name = "Tag12", ThreeLetterIsoCode = "ZZ4"},
            new Tag{Name = "Tag13", ThreeLetterIsoCode = "ZZ5"},
        };
    }
}
