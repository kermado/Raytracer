using System;

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
    }
}
