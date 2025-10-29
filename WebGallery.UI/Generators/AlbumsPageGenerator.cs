using System.Collections.Generic;
using WebGallery.UI.Generators.Helpers;
using WebGallery.UI.ViewModels.Albums;

namespace WebGallery.UI.Generators
{
    public static class AlbumsPageGenerator
    {
        public static AlbumsViewModel SetDisplayProperties(List<AlbumViewModel> input)
        {
            int totalSizeOfRow = 0, indexer = 0;
            var rowFormat = RandomHelpers.GetRandomRowFormat;
            foreach (var gallery in input)
            {
                if (totalSizeOfRow == 12)
                {
                    totalSizeOfRow = 0;
                    indexer = 0;
                    rowFormat = RandomHelpers.GetRandomRowFormat;
                }

                var size = rowFormat[indexer];
                gallery.LargeScreenSize = size;
                gallery.PopUpDelay = 100 * indexer;
                
                totalSizeOfRow += size;
                indexer++;
            }

            return new AlbumsViewModel
            {
                Albums = input
            };
        }
    }
}
