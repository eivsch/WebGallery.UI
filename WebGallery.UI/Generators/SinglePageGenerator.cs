using Application.Galleries;
using Application.Pictures;
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
        public static SingleGalleryViewModel GenerateRandom_ByFolderOrder(string galleryId, IEnumerable<PictureResponse> input)
        {
            input = input.OrderBy(i => i.FolderSortOrder);
            var outList = new List<SingleGalleryImageViewModel>();

            int totalSizeOfRow = 0, indexer = 0;
            var rowFormat = RandomHelpers.GetRandomRowFormat;
            foreach (var item in input)
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
                    Id = item.Id,
                    GalleryIndex = item.FolderSortOrder,
                    Index = item.GlobalSortOrder,
                    LargeScreenSize = size,
                    PopUpDelay = 100 * indexer,
                };

                outList.Add(vm);

                totalSizeOfRow += size;
                indexer++;
            }

            return new SingleGalleryViewModel
            {
                Id = galleryId,
                Images = outList
            };
        }

        public static SingleGalleryViewModel Generate(GalleryResponse input)
        {
            var outList = new List<SingleGalleryImageViewModel>();

            int totalSizeOfRow = 0, indexer = 0;
            var rowFormat = RandomHelpers.GetRandomRowFormat;
            foreach (var item in input.GalleryPictures)
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
                    Id = item.Id,
                    Index = item.Index,
                    LargeScreenSize = size,
                    PopUpDelay = 100 * indexer,
                };

                outList.Add(vm);

                totalSizeOfRow += size;
                indexer++;
            }

            return new SingleGalleryViewModel
            {
                Id = input.Id,
                Images = outList
            };
        }
    }
}
