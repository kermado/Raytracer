using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Raytracer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap bitmap;

        public MainWindow()
        {
            InitializeComponent();

            CreateBitmap();
            UpdateBitmap();
        }

        private void Render()
        {
            // TODO:
        }

        private unsafe void UpdateBitmap()
        {
            this.bitmap.Lock();

            try
            {
                var width = this.bitmap.PixelWidth;
                var height = this.bitmap.PixelHeight;

                var backBuffer = this.bitmap.BackBuffer;
                for (int row = 0; row < height; ++row)
                {
                    for (int col = 0; col < width; ++col)
                    {
                        var ptr = backBuffer + this.bitmap.BackBufferStride * row + col * 3;
                        *((int*)ptr) = row; // TODO: Fill with pixel data.
                    }
                }

                this.bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            }
            finally
            {
                this.bitmap.Unlock();
            }
        }

        private void CreateBitmap()
        {
            this.bitmap = new WriteableBitmap((int)Width, (int)Height, 96, 96, PixelFormats.Rgb24, null);
            Canvas.Source = this.bitmap;
        }
    }
}
