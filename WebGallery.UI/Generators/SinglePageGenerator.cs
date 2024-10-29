using Application.Enums;
using Application.Galleries;
using Infrastructure.MinimalApi;
using System;
using System.Collections.Generic;
using System.IO;
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
                    AppPath = galleryItem.AppPath,
                    GalleryIndex = galleryItem.IndexGlobal ?? -1,
                    IndexGlobal = galleryItem.IndexGlobal ?? -1,
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
                Images = outList,
                GalleryTitle = galleryResponse.GalleryName
            };
        }

        public static SingleGalleryViewModel SetDisplayProperties(List<SingleGalleryImageViewModel> items)
        {
            var outList = new List<SingleGalleryImageViewModel>();

            int totalSizeOfRow = 0, indexer = 0;
            var rowFormat = RandomHelpers.GetRandomRowFormat;
            foreach (var item in items)
            {
                if (totalSizeOfRow == 12)
                {
                    totalSizeOfRow = 0;
                    indexer = 0;
                    rowFormat = RandomHelpers.GetRandomRowFormat;
                }

                var size = rowFormat[indexer];
                item.LargeScreenSize = size;
                item.PopUpDelay = 100 * indexer;

                outList.Add(item);

                totalSizeOfRow += size;
                indexer++;
            }

            return new SingleGalleryViewModel
            {
                Id = "Placeholder Id",
                GalleryTitle = "Placeholder name",
                Images = outList,
            };
        }
    }
}
