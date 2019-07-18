using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;

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

            KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            const float speed = 0.1F;

            if (e.Key == Key.Up || e.Key == Key.W)
            {
                this.camera.Position += this.camera.Forwards * speed;
            }
            else if (e.Key == Key.Down || e.Key == Key.S)
            {
                this.camera.Position -= this.camera.Forwards * speed;
            }
            else if (e.Key == Key.Right || e.Key == Key.D)
            {
                this.camera.Position += this.camera.Right * speed;
            }
            else if (e.Key == Key.Left || e.Key == Key.A)
            {
                this.camera.Position -= this.camera.Right * speed;
            }

            Render();
        }

        private void CreateScene()
        {
            this.scene = new Scene();
            this.scene.Add(new Sphere(new Vector3(0.0F, 0.0F, 4.0F), 1.0F));
            this.scene.Add(new PointLight(new Vector3(0.0F, 4.0F, 0.0F), Color.White, 100.0F));
            this.camera = new PerspectiveCamera();
        }

        private void CreatePixelBuffer()
        {
            this.pixelBuffer = new byte[this.width * this.height * 3];
        }

        private void Render()
        {
            int index = 0;
            for (int row = 0; row < this.height; ++row)
            {
                for (int col = 0; col < this.width; ++col)
                {
                    var color = this.scene.PixelColor(this.camera, col, row, this.width, this.height);
                    this.pixelBuffer[index++] = (byte)(color.R * 255);
                    this.pixelBuffer[index++] = (byte)(color.G * 255);
                    this.pixelBuffer[index++] = (byte)(color.B * 255);
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
