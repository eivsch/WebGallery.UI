using System.Collections.Generic;

namespace WebGallery.UI.ViewModels.Albums
{
    public class AlbumsViewModel
    {
        public List<AlbumViewModel> Albums { get; set; }
        public int TotalAlbumCount { get; set; }
        public int CurrentOffset { get; set; }
        public int DisplayCount { get; set; }
        public bool IsRandomized { get; set; }
        public bool RandomCoverImage { get; set; }
    }
}
