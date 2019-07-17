using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Raytracer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Scene scene;
        private PerspectiveCamera camera;
        private int width;
        private int height;
        private byte[] pixelBuffer;
        private WriteableBitmap bitmap;

        public MainWindow()
        {
            InitializeComponent();

            this.width = (int)Width;
            this.height = (int)Height;

            CreatePixelBuffer();
            CreateBitmap();

            CreateScene();
            Render();
        }

        private void CreateScene()
        {
            this.scene = new Scene();
            this.scene.Add(new Sphere(new Vector3(0.0F, 0.0F, 4.0F), 1.0F));

            this.camera = new PerspectiveCamera();
        }

        private void CreatePixelBuffer()
        {
            this.pixelBuffer = new byte[this.width * this.height * 3];
        }

        private void Render()
        {
            var intersection = new Intersection();

            int index = 0;
            for (int row = 0; row < this.height; ++row)
            {
                for (int col = 0; col < this.width; ++col)
                {
                    if (this.scene.Intersect(this.camera.RayForPixel(col, row, this.width, this.height), ref intersection))
                    {
                        this.pixelBuffer[index++] = 255;
                        this.pixelBuffer[index++] = 255;
                        this.pixelBuffer[index++] = 255;
                    }
                    else
                    {
                        this.pixelBuffer[index++] = 0;
                        this.pixelBuffer[index++] = 0;
                        this.pixelBuffer[index++] = 0;
                    }
                }
            }

            UpdateBitmap();
        }

        private unsafe void UpdateBitmap()
        {
            this.bitmap.Lock();

            try
            {
                byte* ptr = (byte*)this.bitmap.BackBuffer.ToPointer();

                int index = 0;
                for (int row = 0; row < this.height; ++row)
                {
                    for (int col = 0; col < this.width; ++col)
                    {
                        *(ptr++) = this.pixelBuffer[index++];
                        *(ptr++) = this.pixelBuffer[index++];
                        *(ptr++) = this.pixelBuffer[index++];
                    }
                }

                this.bitmap.AddDirtyRect(new Int32Rect(0, 0, this.width, this.height));
            }
            finally
            {
                this.bitmap.Unlock();
            }
        }

        private void CreateBitmap()
        {
            this.bitmap = new WriteableBitmap(this.width, this.height, 96, 96, PixelFormats.Rgb24, null);
            Canvas.Source = this.bitmap;
        }
    }
}
