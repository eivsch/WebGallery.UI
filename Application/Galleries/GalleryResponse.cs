using System.Collections.Generic;

namespace Application.Galleries
{
    public class GalleryResponse
    {
        public string Id { get; set; }
        public int ImageCount { get; set; }
        public IEnumerable<GalleryItem> GalleryItems { get; set; }
    }
}
