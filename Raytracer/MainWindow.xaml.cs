using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Raytracer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Scene scene;
        private PerspectiveCamera camera;
        private uint width;
        private uint height;
        private uint samplesSqrt;
        private byte[] pixelBuffer;
        private WriteableBitmap bitmap;
        private Random random;

        private readonly struct Window
        {
            public readonly uint ystart;
            public readonly uint yend;
            public readonly uint xstart;
            public readonly uint xend;

            public Window(uint ystart, uint yend, uint xstart, uint xend)
            {
                this.ystart = ystart;
                this.yend = yend;
                this.xstart = xstart;
                this.xend = xend;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            this.width = (uint)Width;
            this.height = (uint)Height;
            this.samplesSqrt = 2;
            this.random = new Random();

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

            this.camera = new PerspectiveCamera();
            this.camera.Position = new Vector3(0.0F, 4.0F, -20.0F);
            this.camera.LookAt(new Vector3(0.0F, 1.0F, 0.0F), Vector3.UnitY);
            */

            //this.scene.Add(new Plane(new Vector3(0.0F, 2.0F, 0.0F), new Vector3(0.0F, -1.0F, 0.0F), new Vector3(1.0F, 0.0F, 0.0F), Material.Checkerboard));
            //this.scene.Add(new Plane(new Vector3(0.0F, 0.0F, 0.0F), new Vector3(0.0F, 1.0F, 0.0F), new Vector3(1.0F, 0.0F, 0.0F), Material.Glass));
            //this.scene.Add(new PointLight(new Vector3(0.0F, 1.0F, 10.0F), Color.White, 2000.0F));

            //this.scene.Add(new Sphere(new Vector3(0.0F, 1.0F, 0.0F), 4.0F, new Material(Color.Black, Color.Black, Color.Black, 1.0F, 25.0F, 0.0F, 0.0F, 1.0F, Texture.FromImage("Earth.png"), null)));
            //this.scene.Add(new PointLight(new Vector3(0.0F, 8.0F, -8.0F), Color.White, 2000.0F));

            /*
            this.scene.Add(new Sphere(new Vector3(0.0F, 1.0F, 0.0F), 1.0F, new Material(Color.Black, Color.White, new Color(0.1F, 0.1F, 0.1F), 1.0F, 10.0F, 0.0F, 0.0F, 1.0F, new Vector2(2.0F, 1.0F), Texture.FromImage("MetalPlateDiffuseMap.jpg"), Texture.FromImage("MetalPlateNormalMap.jpg"))));
            this.scene.Add(new PointLight(new Vector3(8.0F, 8.0F, -8.0F), Color.White, 2000.0F));
            this.scene.Add(new PointLight(new Vector3(-8.0F, 8.0F, -8.0F), Color.White, 2000.0F));
            this.scene.Add(new PointLight(new Vector3(0.0F, 8.0F, -8.0F), Color.White, 1000.0F));

            this.scene.Add(new Plane(new Vector3(0.0F, 0.0F, 0.0F), new Vector3(0.0F, 1.0F, 0.0F), new Vector3(1.0F, 0.0F, 0.0F), Material.Checkerboard));
            //this.scene.Add(new Plane(new Vector3(0.0F, 10.0F, 0.0F), new Vector3(0.0F, -1.0F, 0.0F), new Vector3(1.0F, 0.0F, 0.0F), Material.Default));
            //this.scene.Add(new Plane(new Vector3(0.0F, 0.0F, 10.0F), new Vector3(0.0F, 0.0F, -1.0F), new Vector3(1.0F, 0.0F, 0.0F), Material.Default));
            //this.scene.Add(new Plane(new Vector3(-10.0F, 0.0F, 0.0F), new Vector3(1.0F, 0.0F, 0.0F), new Vector3(0.0F, 0.0F, 1.0F), Material.Red));
            //this.scene.Add(new Plane(new Vector3(10.0F, 0.0F, 0.0F), new Vector3(-1.0F, 0.0F, 0.0F), new Vector3(0.0F, 0.0F, 1.0F), Material.Green));

            this.camera = new PerspectiveCamera();
            this.camera.Position = new Vector3(0.0F, 2.0F, -5.0F);
            this.camera.LookAt(new Vector3(0.0F, 1.0F, 0.0F), Vector3.UnitY);
            */

            // Normal map test
            
            this.scene.Add(new Plane(Vector3.Zero, Vector3.UnitY, Vector3.UnitX, new Material(Color.Black, Color.White, Color.Black, 1.0F, 0.0F, 0.0F, 0.0F, 1.0F, Vector2.One, null, Texture.FromImage("TestNormalMap.png"))));
            this.scene.Add(new PointLight(new Vector3(0.0F, 10.0F, 0.0F), Color.White, 2000.0F));

            this.camera = new PerspectiveCamera();
            this.camera.Position = new Vector3(0.0F, 10.0F, -10.0F);
            this.camera.LookAt(new Vector3(0.0F, 0.0F, 0.0F), Vector3.UnitY);
            
        }

        private void CreatePixelBuffer()
        {
            this.pixelBuffer = new byte[this.width * this.height * 3];
        }

        private void Render()
        {
            uint maxWindowHeight = 20;
            uint maxWindowWidth = 20;

            var windows = new List<Window>();

            uint ystart = 0;
            while (ystart < this.height)
            {
                uint yend = Math.Min(ystart + maxWindowHeight, this.height);

                uint xstart = 0;
                while (xstart < this.width)
                {
                    uint xend = Math.Min(xstart + maxWindowWidth, this.width);
                    windows.Add(new Window(ystart, yend, xstart, xend));
                    xstart = xend;
                }

                ystart = yend;
            }

            windows.Shuffle(this.random);

            foreach (var window in windows)
            {
                Task.Run(() => RenderWindow(window));
            }
        }

        private void RenderWindow(Window window)
        {
            for (uint y = window.ystart; y < window.yend; ++y)
            {
                var index = this.width * y * 3 + window.xstart * 3;
                for (uint x = window.xstart; x < window.xend; ++x)
                {
                    var color = this.scene.PixelColor(this.camera, x, y, this.width, this.height, this.samplesSqrt);
                    this.pixelBuffer[index++] = (byte)(color.R * 255);
                    this.pixelBuffer[index++] = (byte)(color.G * 255);
                    this.pixelBuffer[index++] = (byte)(color.B * 255);
                }
            }

            this.bitmap.Dispatcher.Invoke(new Action(() => UpdateBitmapWindow(window)), DispatcherPriority.Render);
        }

        private unsafe void UpdateBitmapWindow(in Window window)
        {
            this.bitmap.Lock();

            try
            {
                var stride = this.bitmap.BackBufferStride;
                byte* startPtr = (byte*)this.bitmap.BackBuffer.ToPointer();
                
                for (uint y = window.ystart; y < window.yend; ++y)
                {
                    var xoffset = window.xstart * 3u;
                    var index = y * this.width * 3u + xoffset;
                    byte* ptr = startPtr + y * stride + xoffset;

                    for (uint x = window.xstart; x < window.xend; ++x)
                    {
                        *(ptr++) = this.pixelBuffer[index++];
                        *(ptr++) = this.pixelBuffer[index++];
                        *(ptr++) = this.pixelBuffer[index++];
                    }
                }

                this.bitmap.AddDirtyRect(new Int32Rect((int)window.xstart, (int)window.ystart, (int)(window.xend - window.xstart), (int)(window.yend - window.ystart)));
            }
            finally
            {
                this.bitmap.Unlock();
            }
        }

        private void CreateBitmap()
        {
            this.bitmap = new WriteableBitmap((int)this.width, (int)this.height, 96, 96, PixelFormats.Rgb24, null);
            Canvas.Source = this.bitmap;
        }
    }
}