using System;
using System.Drawing;
using System.Numerics;

namespace Raytracer
{
    public class Texture
    {
        private readonly int width;
        private readonly int height;
        private readonly float tilex;
        private readonly float tiley;
        private readonly Color[] pixels;

        private Texture(int width, int height, float tilex, float tiley)
        {
            this.width = width;
            this.height = height;
            this.tilex = tilex;
            this.tiley = tiley;
            this.pixels = new Color[width * height];
        }

        public Texture(int width, int height, float tilex, float tiley, Color[] pixels)
        {
            this.width = width;
            this.height = height;
            this.tilex = tilex;
            this.tiley = tiley;
            this.pixels = pixels;
        }

        public Color Color(in Vector2 uv)
        {
            // Apply tiling.
            float u = uv.X * tilex;
            float v = uv.Y * tiley;

            // Normalize to [0, 1)
            float x = u - (float)Math.Floor(u);
            float y = v - (float)Math.Floor(v);

            // Determine pixel coordinate.
            float px = x * this.width;
            float py = y * this.height;

            return this.pixels[(int)py * this.width + (int)px];
        }

        private static Color BilinearInterpolate(float tx, float ty, in Color c00, in Color c10, in Color c01, in Color c11)
        {
            // C00                       a         C10
            //  +------------------------+----------+
            //  |           ^            |          |
            //  |           | ty         |
            //  |           |            |          |
            //  |           v            | p        |
            //  +------------------------+----------+
            //  |                        |          |
            //  |<---------------------->|          |
            //  |           tx           |          |
            //  |                        |          |
            //  |                        |          |
            //  |                        |          |
            //  |                        |          |
            //  |                        |          |
            //  +------------------------+----------+
            // C01                       b         C11

            var temp = (1.0F - tx);
            var a = c00 * temp + c10 * tx;
            var b = c01 * temp + c11 * tx;
            return a * (1.0F - ty) + b * ty;
        }

        public Color BilinearFilteredColor(in Vector2 uv)
        {
            // Apply tiling.
            float u = uv.X * tilex;
            float v = uv.Y * tiley;

            // Normalize to [0, 1)
            float x = u - (float)Math.Floor(u);
            float y = v - (float)Math.Floor(v);

            // Determine texel colors.
            float px = x * this.width;
            float py = y * this.height;
            int px0 = (int)Math.Floor(px) % this.width;
            int px1 = (int)Math.Ceiling(px) % this.width;
            int py0 = (int)Math.Floor(py) % this.height;
            int py1 = (int)Math.Ceiling(py) % this.height;
            Color c00 = this.pixels[py0 * this.width + px0];
            Color c10 = this.pixels[py0 * this.width + px1];
            Color c01 = this.pixels[py1 * this.width + px0];
            Color c11 = this.pixels[py1 * this.width + px1];

            // Interpolate.
            float tx = px - (float)Math.Floor(px);
            float ty = py - (float)Math.Floor(py);
            return BilinearInterpolate(tx, ty, c00, c10, c01, c11);
        }

        public static unsafe Texture FromImage(string filepath, float tilex = 1, float tiley = 1)
        {
            using (var image = new Bitmap(filepath))
            {
                if (image != null)
                {
                    var width = image.Width;
                    var height = image.Height;
                    var texture = new Texture(width, height, tilex, tiley);

                    var sourceData = image.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    var ptr = (byte*)sourceData.Scan0;

                    int shift = sourceData.Stride - width * 3;
                    int index = 0;
                    for (int y = 0; y < height; ++y)
                    {
                        for (int x = 0; x < width; ++x)
                        {
                            byte b = *(ptr++);
                            byte g = *(ptr++);
                            byte r = *(ptr++);
                            texture.pixels[index++] = new Color((float)r / 255.0F, (float)g / 255.0F, (float)b / 255.0F);
                        }

                        ptr += shift;
                    }

                    image.UnlockBits(sourceData);

                    return texture;
                }

                return null;
            }
        }

        public static Texture Checkerboard(int rows, int cols, int size, Color color1, Color color2)
        {
            int width = cols * size;
            int height = rows * size;
            var texture = new Texture(width, height, 1, 1);

            for (int row = 0; row < rows; ++row)
            {
                int ystart = row * size;
                int yend = ystart + size;

                for (int col = 0; col < cols; ++col)
                {
                    var color = (((row + col) % 2) == 0) ? color1 : color2;

                    int xstart = col * size;
                    int xend = xstart + size;
                    
                    for (int y = ystart; y < yend; ++y)
                    {
                        for (int x = xstart; x < xend; ++x)
                        {
                            texture.pixels[y * width + x] = color;
                        }
                    }
                }
            }

            return texture;
        }
    }
}
