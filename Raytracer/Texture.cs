using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;

namespace Raytracer
{
    public class Texture
    {
        private readonly struct Window
        {
            public readonly uint xstart;
            public readonly uint xend;
            public readonly uint ystart;
            public readonly uint yend;

            public Window(uint xstart, uint xend, uint ystart, uint yend)
            {
                this.xstart = xstart;
                this.xend = xend;
                this.ystart = ystart;
                this.yend = yend;
            }
        }

        private readonly uint width;
        private readonly uint height;
        private readonly uint dimension;
        private readonly uint fullwidth;
        private readonly uint levels;
        private readonly Color[] pixels;

        private Texture(uint width, uint height)
        {
            this.width = width;
            this.height = height;

            this.dimension = Util.NextPow2(Math.Max(this.width, this.height));
            this.fullwidth = this.dimension + this.dimension / 2u;
            this.levels = 1u + (uint)Math.Log(dimension, 2);
            this.pixels = new Color[this.dimension * (this.dimension + this.dimension / 2)];
        }

        public Texture(uint width, uint height, Color[] pixels) : this(width, height)
        {
            // Copy pixel colors from supplied array to level 0 of the mipmap.
            for (uint y = 0; y < height; ++y)
            {
                uint yoffset = y * this.dimension;
                for (uint x = 0; x < width; ++x)
                {
                   this.pixels[yoffset + x] = pixels[y * width + x];
                }
            }

            CreateMipmap();
        }

        public Bitmap CreateBitmap()
        {
            var bitmap = new Bitmap((int)this.fullwidth, (int)this.dimension);
            for (int y = 0; y < this.dimension; ++y)
            {
                for (int x = 0; x < this.fullwidth; ++x)
                {
                    var color = this.pixels[y * this.fullwidth + x];
                    bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(255, (int)(255 * color.R), (int)(255 * color.G), (int)(255 * color.B)));
                }
            }

            return bitmap;
        }

        public void Save(string filepath)
        {
            var ext = Path.GetExtension(filepath);
            var format = ImageFormat.Bmp;
            switch (ext)
            {
                case ".bmp":  format = ImageFormat.Bmp; break;
                case ".emf":  format = ImageFormat.Emf; break;
                case ".wmf":  format = ImageFormat.Wmf; break;
                case ".gif":  format = ImageFormat.Gif; break;
                case ".jpeg":
                case ".jpg":  format = ImageFormat.Jpeg; break;
                case ".png":  format = ImageFormat.Png; break;
                case ".tiff":
                case ".tif":  format = ImageFormat.Tiff; break;
                case ".exif": format = ImageFormat.Exif; break;
                case ".ico":  format = ImageFormat.Icon; break;
                default: throw new ArgumentException("Invalid/unknown file extension.");
            }

            var bitmap = CreateBitmap();
            bitmap.Save(filepath, format);
        }

        private void CreateMipmap()
        {
            for (uint i = 1; i < this.levels; ++i)
            {
                CreateLevel(i);
            }
        }

        /// <summary>
        /// Determines the window into the texture for the specified mipmap level.
        /// </summary>
        /// <param name="level">The mipmap level.</param>
        /// <returns>The window into the texture.</returns>
        private Window LevelWindow(uint level)
        {
            if (level <= 0) { return new Window(0, this.width, 0, this.height); }

            var previousWindow = LevelWindow(level - 1); // TODO: Don't recurse.
            var previousWidth = previousWindow.xend - previousWindow.xstart;
            var previousHeight = previousWindow.yend - previousWindow.ystart;

            uint ystart = this.dimension - (this.dimension / (uint)Util.Pow(2, level - 1u));
            uint yend = ystart + previousHeight / 2;
            uint xstart = this.dimension;
            uint xend = xstart + previousWidth / 2;

            return new Window(xstart, xend, ystart, yend);
        }

        private void CreateLevel(uint level)
        {
            if (level < this.levels)
            {
                var previousWindow = LevelWindow(level - 1);
                var currentWindow = LevelWindow(level);
                var currentWindowWidth = currentWindow.yend - currentWindow.ystart;
                var currentWindowHeight = currentWindow.xend - currentWindow.xstart;

                for (uint v = 0; v < currentWindowHeight; ++v)
                {
                    uint y = currentWindow.ystart + v;
                    uint py = previousWindow.ystart + v * 2;

                    for (uint h = 0; h < currentWindowWidth; ++h)
                    {
                        uint x = currentWindow.xstart + h;
                        uint px = previousWindow.xstart + h * 2;

                        uint tl = py * this.fullwidth + px; // Index for the top-left pixel in the previous window.
                        uint bl = (py + 1) * this.fullwidth + px; // Index for the bottom-left pixel in the previous window.

                        this.pixels[y * this.fullwidth + x] = (this.pixels[tl] + this.pixels[tl + 1] + this.pixels[bl] + this.pixels[bl + 1]) * 0.25F;
                    }
                }
            }
        }

        public Color Color(in Vector2 uv, in Vector2 tile)
        {
            float u = uv.X * tile.X;
            float v = uv.Y * tile.Y;

            // Normalize to [0, 1)
            float x = u - (float)Math.Floor(u);
            float y = v - (float)Math.Floor(v);

            // Determine pixel coordinate.
            float px = x * this.width;
            float py = y * this.height;

            return this.pixels[(uint)py * this.fullwidth + (uint)px];
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

        public Color BilinearFilteredColor(in Vector2 uv, in Vector2 tile)
        {
            // Apply tiling.
            float u = uv.X * tile.X;
            float v = uv.Y * tile.Y;

            // Normalize to [0, 1)
            float x = u - (float)Math.Floor(u);
            float y = v - (float)Math.Floor(v);

            // Determine texel colors.
            float px = x * this.width;
            float py = y * this.height;
            uint px0 = (uint)(px) % this.width;
            uint px1 = (px0 + 1) % this.width;
            uint py0 = (uint)(py) % this.height;
            uint py1 = (py0 + 1) % this.height;

            uint ro0 = py0 * this.fullwidth;
            uint ro1 = py1 * this.fullwidth;

            var c00 = this.pixels[ro0 + px0];
            var c10 = this.pixels[ro0 + px1];
            var c01 = this.pixels[ro1 + px0];
            var c11 = this.pixels[ro1 + px1];

            // Interpolate.
            float tx = px - (float)Math.Floor(px);
            float ty = py - (float)Math.Floor(py);
            return BilinearInterpolate(tx, ty, c00, c10, c01, c11);
        }

        public static unsafe Texture FromImage(string filepath)
        {
            using (var image = new Bitmap(filepath))
            {
                if (image != null)
                {
                    var width = image.Width;
                    var height = image.Height;
                    var texture = new Texture((uint)width, (uint)height);

                    var sourceData = image.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    var ptr = (byte*)sourceData.Scan0;

                    int shift = sourceData.Stride - width * 3;
                    for (uint y = 0; y < height; ++y)
                    {
                        uint xstart = texture.fullwidth * y;
                        for (int x = 0; x < width; ++x)
                        {
                            byte b = *(ptr++);
                            byte g = *(ptr++);
                            byte r = *(ptr++);
                            texture.pixels[xstart + x] = new Color((float)r / 255.0F, (float)g / 255.0F, (float)b / 255.0F);
                        }

                        ptr += shift;
                    }

                    image.UnlockBits(sourceData);

                    texture.CreateMipmap();
                    return texture;
                }

                return null;
            }
        }

        public static Texture Checkerboard(uint rows, uint cols, uint size, Color color1, Color color2)
        {
            uint width = cols * size;
            uint height = rows * size;
            var texture = new Texture(width, height);

            for (uint row = 0; row < rows; ++row)
            {
                uint ystart = row * size;
                uint yend = ystart + size;

                for (uint col = 0; col < cols; ++col)
                {
                    var color = (((row + col) % 2) == 0) ? color1 : color2;

                    uint xstart = col * size;
                    uint xend = xstart + size;
                    
                    for (uint y = ystart; y < yend; ++y)
                    {
                        for (uint x = xstart; x < xend; ++x)
                        {
                            texture.pixels[y * texture.fullwidth + x] = color;
                        }
                    }
                }
            }

            texture.CreateMipmap();
            return texture;
        }
    }
}
