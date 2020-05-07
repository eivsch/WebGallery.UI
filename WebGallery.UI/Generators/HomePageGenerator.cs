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

        static HomeViewModel GenerateAllRandom(IEnumerable<GalleryResponse> input)
        {
            var outputList = new List<HomeGalleryViewModel>();

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

                outputList.Add(
                    new HomeGalleryViewModel
                    {
                        Id = item.Id,
                        ItemCount = item.ImageCount,
                        CoverImageUrl = $"{_baseUrl}/{item.Id}/1",   // TODO: Randomize cover image
                        LargeScreenSize = rowFormat[indexer++]
                    });
            }

            return null;
        }

        private static int[] GetRandomRowFormat()
        {
            int i = rnd.Next(1, 5);

            switch (i)
            {
                case 1:
                    return new int[] { 4, 4, 4 };
                case 2:
                    return new int[] { 4, 8 };
                case 3:
                    return new int[] { 6, 6 };
                case 4:
                    return new int[] { 8, 4 };
                default:
                    throw new Exception("Invalid row format requested");
            }
        }
    }
}
