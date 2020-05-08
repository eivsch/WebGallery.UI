using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebGallery.UI.Generators.Helpers
{
    public static class RandomExtensions
    {
        public static void Shuffle<T>(this IList<T> list, Random rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
