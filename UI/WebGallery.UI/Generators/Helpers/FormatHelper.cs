using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebGallery.UI.Generators.Helpers
{
    public static class FormatHelper
    {
        public static int[][] Format => new int[][]
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
    }
}
