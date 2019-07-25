using System;
using System.Drawing;
using System.Numerics;

namespace Raytracer
{
    public class Texture
    {
        private readonly int width;
        private readonly int height;
        private readonly int tilex;
        private readonly int tiley;
        private readonly Color[] pixels;

        private Texture(int width, int height, int tilex, int tiley)
        {
            this.width = width;
            this.height = height;
            this.tilex = tilex;
            this.tiley = tiley;
            this.pixels = new Color[width * height];
        }

        public Texture(int width, int height, int tilex, int tiley, Color[] pixels)
        {
            this.width = width;
            this.height = height;
            this.tilex = tilex;
            this.tiley = tiley;
            this.pixels = pixels;
        }

        private static float Mod(float a, float b)
        {
            return a - b * (float)Math.Floor(a / b);
        }

        public Color Color(in Vector2 uv)
        {
            float px = Mod(uv.X * tilex, 1.0F) * this.width;
            float py = Mod(uv.Y * tiley, 1.0F) * this.height;

            return this.pixels[(int)py * this.width + (int)px];
        }

        public static unsafe Texture FromImage(string filepath, int tilex = 1, int tiley = 1)
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
