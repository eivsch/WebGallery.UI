using Application.Galleries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebGallery.UI.ViewModels;

namespace WebGallery.UI.Generators
{
    public static class HomePageGenerator
    {
        static Random rnd = new Random();

        private static string _baseUrl = "http://localhost:5000/pictures";

        public static HomeViewModel GenerateAllRandom(IEnumerable<GalleryResponse> input)
        {
            var outList = new List<HomeGalleryViewModel>();

            int totalSizeOfRow = 0, indexer = 0;
            var rowFormat = GetRandomRowFormat();
            foreach (var item in input)
            {
                if (totalSizeOfRow == 12)
                {
                    totalSizeOfRow = 0;
                    indexer = 0;
                    rowFormat = GetRandomRowFormat();
                }

                var size = rowFormat[indexer];
                var coverImageIndex = rnd.Next(1, item.ImageCount);
                var vm = new HomeGalleryViewModel
                {
                    Id = item.Id,
                    ItemCount = item.ImageCount,
                    CoverImageIndex = coverImageIndex,
                    CoverImageUrl = $"{_baseUrl}/{item.Id}/{coverImageIndex}",     // TODO: Move this concatenation into the view instead?
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

        private static int[] GetRandomRowFormat()
        {
            var all = new int[][]
            {
                // Valid with 4 items
                new int[] { 3, 3, 3, 3 },
                // Valid with 3 items
                new int[] { 4,4,4 },
                new int[] { 6,3,3 },
                new int[] { 3,6,3 },
                new int[] { 3,3,6 },
                // Valid with 2 items
                new int[] { 6,6 },
                new int[] { 4,8 },
                new int[] { 8,4 },
            };

            return all[rnd.Next(0, all.Length)];
        }
    }
}
