using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
        private CancellationTokenSource tokenSource;

        public MainWindow()
        {
            InitializeComponent();

            this.width = (int)Width;
            this.height = (int)Height;

            CreatePixelBuffer();
            CreateBitmap();

            CreateScene();

            Loaded += OnLoaded;
            KeyDown += OnKeyDown;
            MouseUp += OnMouseUp;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Render();
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(Canvas);
            Console.WriteLine(pos);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            const float speed = 0.5F;

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

            //this.scene.Add(new Sphere(new Vector3(0.0F, 1.0F, 6.0F), 1.0F, Material.Default));
            //this.scene.Add(new Plane(new Vector3(0.0F, 0.0F, 15.0F), new Vector3(0.0F, 0.0F, -1.0F), Material.Default));
            //this.scene.Add(new Sphere(new Vector3(1.0F, 1.0F, 4.0F), 1.0F, Material.Glass));
            //this.scene.Add(new Plane(new Vector3(0.0F, -1.0F, 0.0F), new Vector3(0.0F, 1.0F, 0.0F)));

            /*
            this.scene.Add(new Sphere(new Vector3(0.0F, 0.0F, -2.0F), 1.0F, Material.Mirror));
            this.scene.Add(new Sphere(new Vector3(-2.0F, 0.0F, -2.0F), 1.0F, Material.Mirror));
            this.scene.Add(new Sphere(new Vector3(2.0F, 0.0F, -2.0F), 1.0F, Material.Mirror));

            this.scene.Add(new Sphere(new Vector3(0.0F, 0.0F, 0.0F), 1.0F, Material.Mirror));
            this.scene.Add(new Sphere(new Vector3(-2.0F, 0.0F, 0.0F), 1.0F, Material.Mirror));
            this.scene.Add(new Sphere(new Vector3(2.0F, 0.0F, 0.0F), 1.0F, Material.Mirror));

            this.scene.Add(new Sphere(new Vector3(0.0F, 0.0F, 2.0F), 1.0F, Material.Mirror));
            this.scene.Add(new Sphere(new Vector3(-2.0F, 0.0F, 2.0F), 1.0F, Material.Mirror));
            this.scene.Add(new Sphere(new Vector3(2.0F, 0.0F, 2.0F), 1.0F, Material.Mirror));

            this.scene.Add(new Sphere(new Vector3(1.0F, (float)Math.Sqrt(2.0F), -1.0F), 1.0F, Material.Mirror));
            this.scene.Add(new Sphere(new Vector3(-1.0F, (float)Math.Sqrt(2.0F), -1.0F), 1.0F, Material.Mirror));

            this.scene.Add(new Sphere(new Vector3(1.0F, (float)Math.Sqrt(2.0F), 1.0F), 1.0F, Material.Mirror));
            this.scene.Add(new Sphere(new Vector3(-1.0F, (float)Math.Sqrt(2.0F), 1.0F), 1.0F, Material.Mirror));

            this.scene.Add(new Sphere(new Vector3(0.0F, (float)Math.Sqrt(2.0F) * 2.0F, 0.0F), 1.0F, Material.Mirror));

            this.scene.Add(new Plane(new Vector3(0.0F, -1.0F, 0.0F), new Vector3(0.0F, 1.0F, 0.0F), new Vector3(1.0F, 0.0F, 0.0F), Material.Checkerboard));
            this.scene.Add(new Plane(new Vector3(0.0F, 0.0F, 15.0F), new Vector3(0.0F, 0.0F, -1.0F), new Vector3(1.0F, 0.0F, 0.0F), Material.Default));
            this.scene.Add(new Plane(new Vector3(-15.0F, 0.0F, 0.0F), new Vector3(1.0F, 0.0F, 0.0F), new Vector3(0.0F, 0.0F, 1.0F), Material.Red));
            this.scene.Add(new Plane(new Vector3(15.0F, 0.0F, 0.0F), new Vector3(-1.0F, 0.0F, 0.0F), new Vector3(0.0F, 0.0F, 1.0F), Material.Green));

            this.scene.Add(new Sphere(new Vector3(8.0F, 0.0F, 0.0F), 1.0F, Material.Glass));
            this.scene.Add(new Sphere(new Vector3(8.0F, 2.0F, 0.0F), 1.0F, Material.Glass));
            this.scene.Add(new Sphere(new Vector3(8.0F, 4.0F, 0.0F), 1.0F, Material.Glass));

            this.scene.Add(new Sphere(new Vector3(-8.0F, 0.0F, 0.0F), 1.0F, Material.Glass));
            this.scene.Add(new Sphere(new Vector3(-8.0F, 2.0F, 0.0F), 1.0F, Material.Glass));
            this.scene.Add(new Sphere(new Vector3(-8.0F, 4.0F, 0.0F), 1.0F, Material.Glass));

            this.scene.Add(new PointLight(new Vector3(-8.0F, 8.0F, 0.0F), Color.White, 2000.0F));
            this.scene.Add(new PointLight(new Vector3(8.0F, 8.0F, 0.0F), Color.White, 2000.0F));
            this.scene.Add(new PointLight(new Vector3(0.0F, 8.0F, -8.0F), Color.White, 2000.0F));
            */

            //this.scene.Add(new Plane(new Vector3(0.0F, 2.0F, 0.0F), new Vector3(0.0F, -1.0F, 0.0F), new Vector3(1.0F, 0.0F, 0.0F), Material.Checkerboard));
            //this.scene.Add(new Plane(new Vector3(0.0F, 0.0F, 0.0F), new Vector3(0.0F, 1.0F, 0.0F), new Vector3(1.0F, 0.0F, 0.0F), Material.Glass));
            //this.scene.Add(new PointLight(new Vector3(0.0F, 1.0F, 10.0F), Color.White, 2000.0F));

            //this.scene.Add(new Sphere(new Vector3(0.0F, 1.0F, 0.0F), 4.0F, new Material(Color.Black, Color.Black, Color.Black, 1.0F, 25.0F, 0.0F, 0.0F, 1.0F, Texture.FromImage("Earth.png"), null)));
            //this.scene.Add(new PointLight(new Vector3(0.0F, 8.0F, -8.0F), Color.White, 2000.0F));

            this.scene.Add(new Sphere(new Vector3(0.0F, 1.0F, 0.0F), 1.0F, new Material(Color.Black, Color.Black, Color.White, 1.0F, 50.0F, 0.6F, 0.0F, 1.0F, Texture.FromImage("MetalWeaveDiffuseMap.jpg", 2, 1), Texture.FromImage("MetalWeaveNormalMap.jpg", 2, 1))));
            this.scene.Add(new PointLight(new Vector3(8.0F, 8.0F, -8.0F), Color.White, 2000.0F));
            this.scene.Add(new PointLight(new Vector3(-8.0F, 8.0F, -8.0F), Color.White, 2000.0F));
            //this.scene.Add(new PointLight(new Vector3(8.0F, 8.0F, 0.0F), Color.White, 1000.0F));
            //this.scene.Add(new PointLight(new Vector3(0.0F, 8.0F, -8.0F), Color.White, 1000.0F));

            this.scene.Add(new Plane(new Vector3(0.0F, 0.0F, 0.0F), new Vector3(0.0F, 1.0F, 0.0F), new Vector3(1.0F, 0.0F, 0.0F), Material.Checkerboard));
            this.scene.Add(new Plane(new Vector3(0.0F, 10.0F, 0.0F), new Vector3(0.0F, -1.0F, 0.0F), new Vector3(1.0F, 0.0F, 0.0F), Material.Default));
            this.scene.Add(new Plane(new Vector3(0.0F, 0.0F, 10.0F), new Vector3(0.0F, 0.0F, -1.0F), new Vector3(1.0F, 0.0F, 0.0F), Material.Default));
            this.scene.Add(new Plane(new Vector3(-10.0F, 0.0F, 0.0F), new Vector3(1.0F, 0.0F, 0.0F), new Vector3(0.0F, 0.0F, 1.0F), Material.Red));
            this.scene.Add(new Plane(new Vector3(10.0F, 0.0F, 0.0F), new Vector3(-1.0F, 0.0F, 0.0F), new Vector3(0.0F, 0.0F, 1.0F), Material.Green));

            this.camera = new PerspectiveCamera();
            this.camera.Position = new Vector3(0.0F, 4.0F, -10.0F);
            this.camera.LookAt(new Vector3(0.0F, 1.0F, 0.0F), Vector3.UnitY);
        }

        private void CreatePixelBuffer()
        {
            this.pixelBuffer = new byte[this.width * this.height * 3];
        }

        private void Render()
        {
            if (this.tokenSource != null)
            {
                this.tokenSource.Cancel();
                this.tokenSource.Dispose();
            }

            this.tokenSource = new CancellationTokenSource();
            var token = this.tokenSource.Token;

            var taskCount = 20;
            var rowsPerTask = this.height / taskCount;
            var remainder = this.height - rowsPerTask * taskCount;

            for (int i = 0; i < taskCount; ++i)
            {
                int start = i * rowsPerTask;
                int end = start + rowsPerTask;
                if (i == taskCount - 1) { end += remainder; }

                Task.Run(() =>
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        RenderRows(start, end, token);
                        Dispatcher.BeginInvoke(new Action<int, int>(UpdateBitmapRows), start, end);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Cancelled");
                    }
                }, token);
            }
        }

        private void RenderRows(int startRowInclusive, int endRowExclusive, CancellationToken token)
        {
            for (int row = startRowInclusive; row < endRowExclusive; ++row)
            {
                token.ThrowIfCancellationRequested();
                RenderRow(row);
            }
        }

        private void RenderRow(int row)
        {
            var index = this.width * row * 3;
            for (int col = 0; col < this.width; ++col)
            {
                var color = this.scene.PixelColor(this.camera, col, row, this.width, this.height);
                this.pixelBuffer[index++] = (byte)(color.R * 255);
                this.pixelBuffer[index++] = (byte)(color.G * 255);
                this.pixelBuffer[index++] = (byte)(color.B * 255);
            }
        }

        private unsafe void UpdateBitmapRows(int startRowInclusive, int endRowExclusive)
        {
            this.bitmap.Lock();

            try
            {
                byte* ptr = (byte*)this.bitmap.BackBuffer.ToPointer();

                int index = startRowInclusive * this.width * 3;
                ptr += index;

                for (int row = startRowInclusive; row < endRowExclusive; ++row)
                {
                    for (int col = 0; col < this.width; ++col)
                    {
                        *(ptr++) = this.pixelBuffer[index++];
                        *(ptr++) = this.pixelBuffer[index++];
                        *(ptr++) = this.pixelBuffer[index++];
                    }
                }

                this.bitmap.AddDirtyRect(new Int32Rect(0, startRowInclusive, this.width, endRowExclusive - startRowInclusive));
            }
            finally
            {
                this.bitmap.Unlock();
            }
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