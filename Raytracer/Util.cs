using System;
using System.Collections.Generic;

namespace Raytracer
{
    public static class Util
    {
        public static int Pow(int x, uint pow)
        {
            int ret = 1;
            while (pow != 0)
            {
                if ((pow & 1) == 1) { ret *= x; }
                x *= x;
                pow >>= 1;
            }

            return ret;
        }

        public static uint NextPow2(uint n)
        {
            return (uint)Pow(2, (uint)Math.Ceiling(Math.Log(n, 2)));
        }

        public static void Shuffle<T>(this IList<T> list, Random rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                --n;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
