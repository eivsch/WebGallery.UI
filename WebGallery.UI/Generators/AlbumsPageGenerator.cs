using Application.Galleries;
using System.Collections.Generic;
using WebGallery.UI.Generators.Helpers;
using WebGallery.UI.ViewModels.Albums;

namespace WebGallery.UI.Generators
{
    public static class AlbumsPageGenerator
    {
        public static AlbumsViewModel GenerateAllRandom(IList<GalleryResponse> input)
        {
            input.ShuffleList();
            var outList = new List<AlbumViewModel>();

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
                var vm = new AlbumViewModel
                {
                    GalleryId = gallery.Id,
                    ItemCount = gallery.ImageCount,
                    LargeScreenSize = size,
                    PopUpDelay = 100 * indexer,
                    Title = gallery.GalleryName
                };

                outList.Add(vm);
                
                totalSizeOfRow += size;
                indexer++;
            }

            return new AlbumsViewModel
            {
                Albums = outList
            };
        }

    }
}
