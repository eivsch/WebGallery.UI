using System.Collections.Generic;
using WebGallery.UI.Generators.Helpers;
using WebGallery.UI.ViewModels.Single;

namespace WebGallery.UI.Generators
{
    public static class SinglePageGenerator
    {
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
