using Application.Galleries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebGallery.UI.Generators.Helpers;
using WebGallery.UI.ViewModels.Single;

namespace WebGallery.UI.Generators
{
    public static class SinglePageGenerator
    {
        public static SingleGalleryViewModel Generate(GalleryResponse galleryResponse)
        {
            var outList = new List<SingleGalleryImageViewModel>();

            int totalSizeOfRow = 0, indexer = 0;
            var rowFormat = RandomHelpers.GetRandomRowFormat;
            foreach (var galleryItem in galleryResponse.GalleryItems)
            {
                if (totalSizeOfRow == 12)
                {
                    totalSizeOfRow = 0;
                    indexer = 0;
                    rowFormat = RandomHelpers.GetRandomRowFormat;
                }

                var size = rowFormat[indexer];
                var vm = new SingleGalleryImageViewModel
                {
                    Id = galleryItem.Id,
                    GalleryIndex = galleryItem.Index,
                    Index = galleryItem.Index,
                    LargeScreenSize = size,
                    PopUpDelay = 100 * indexer,
                    MediaType = galleryItem.MediaType
                };

                outList.Add(vm);

                totalSizeOfRow += size;
                indexer++;
            }

            return new SingleGalleryViewModel
            {
                Id = galleryResponse.Id,
                Images = outList
            };
        }
    }
}
