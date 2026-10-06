using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebGallery.UI.Generators.Helpers
{
    public static class RandomHelpers
    {
        public static Random Rng { get; } = new Random();

        public static int Next(int n, int m) => Rng.Next(n, m);

        public static int[] GetRandomRowFormat => FormatHelper.Format[Rng.Next(0, FormatHelper.Format.Length)];

        public static void ShuffleList<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
