using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Galleries
{
    public class GalleryResponse
    {
        public string Id { get; set; }
        public int ImageCount { get; set; }
        public IEnumerable<GalleryPicture> GalleryPictures { get; set; }
    }
}
