using Application.Galleries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebGallery.UI.Generators.Helpers;
using WebGallery.UI.ViewModels;

namespace WebGallery.UI.Generators
{
    public static class HomePageGenerator
    {
        public static HomeViewModel GenerateAllRandom(IEnumerable<GalleryResponse> input)
        {
            input.ToList().ShuffleList();
            var outList = new List<HomeGalleryViewModel>();

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
                var coverImageIndex = RandomHelpers.Rng.Next(1, gallery.ImageCount);
                var vm = new HomeGalleryViewModel
                {
                    GalleryId = gallery.Id,
                    ItemCount = gallery.ImageCount,
                    CoverImageIndex = coverImageIndex,
                    LargeScreenSize = size,
                    PopUpDelay = 100 * indexer,
                };

                outList.Add(vm);
                
                totalSizeOfRow += size;
                indexer++;
            }

            return new HomeViewModel
            {
                Galleries = outList
            };
        }

    }
}
