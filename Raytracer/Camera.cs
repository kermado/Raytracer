using System;
using System.Numerics;

namespace Raytracer
{
    public sealed class PerspectiveCamera
    {
        /// <summary>
        /// The transform for the camera.
        /// </summary>
        private Matrix4x4 transform;

        /// <summary>
        /// The vertical field-of-view.
        /// </summary>
        private readonly float vfov;

        /// <summary>
        /// The aspect ratio (with/height).
        /// </summary>
        private readonly float ar;

        /// <summary>
        /// The half-height of the screen door.
        /// </summary>
        private readonly float h;

        /// <summary>
        /// The half-width of the screen door.
        /// </summary>
        private readonly float w;

        /// <summary>
        /// The exposure, used for color correction.
        /// </summary>
        private float exposure;

        /// <summary>
        /// The gamma, used for color correction.
        /// </summary>
        private float gamma;

        /// <summary>
        /// The right direction for the camera.
        /// </summary>
        public Vector3 Right
        {
            get { return new Vector3(this.transform.M11, this.transform.M12, this.transform.M13); }
        }

        /// <summary>
        /// The up direction for the camera.
        /// </summary>
        public Vector3 Up
        {
            get { return new Vector3(this.transform.M21, this.transform.M22, this.transform.M23); }
        }

        /// <summary>
        /// The direction in which the camera is facing.
        /// </summary>
        public Vector3 Forwards
        {
            get { return new Vector3(this.transform.M31, this.transform.M32, this.transform.M33); }
        }

        /// <summary>
        /// The position for the camera.
        /// </summary>
        public Vector3 Position
        {
            get { return new Vector3(this.transform.M41, this.transform.M42, this.transform.M43); }
            set
            {
                this.transform.M41 = value.X;
                this.transform.M42 = value.Y;
                this.transform.M43 = value.Z;
            }
        }

        /// <summary>
        /// The exposure, used for color correction.
        /// </summary>
        public float Exposure
        {
            get { return this.exposure; }
            set { this.exposure = value; }
        }

        /// <summary>
        /// The gamma, used for color correction.
        /// </summary>
        public float Gamma
        {
            get { return this.gamma; }
            set { this.gamma = value; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PerspectiveCamera()
        {
            this.transform = Matrix4x4.Identity;
            this.vfov = (float)(Math.PI * 0.125);
            this.ar = (float)(16.0 / 9.0);
            this.h = (float)Math.Tan(this.vfov);
            this.w = this.h * ar;
            this.exposure = 1.0F;
            this.gamma = 2.2F;            
        }

        /// <summary>
        /// Orients the camera to look at the specified point.
        /// </summary>
        /// <param name="targetPoint">The target point.</param>
        /// <param name="upHint">A hint for the up vector.</param>
        public void LookAt(Vector3 targetPoint, Vector3 upHint)
        {
            var zaxis = Vector3.Normalize(targetPoint - Position);
            var xaxis = Vector3.Normalize(Vector3.Cross(upHint, zaxis));
            var yaxis = Vector3.Normalize(Vector3.Cross(zaxis, xaxis));
            var origin = this.transform.Translation;

            this.transform = new Matrix4x4(xaxis.X,  xaxis.Y,  xaxis.Z,  0.0F,
                                           yaxis.X,  yaxis.Y,  yaxis.Z,  0.0F,
                                           zaxis.X,  zaxis.Y,  zaxis.Z,  0.0F,
                                           origin.X, origin.Y, origin.Z, 1.0F);
        }

        /// <summary>
        /// Creates a ray from the camera that passes through the screen door at the specified point
        /// in screen coordinates.
        /// </summary>
        /// <remarks>
        /// The screen coordinates are in the interval [-1, 1], where the center is at point (0, 0).
        /// The screen door is assumed to be 1m in front of the camera.
        /// </remarks>
        /// <param name="h">The horizontal position in screen coordinates.</param>
        /// <param name="v">The vertical position in screen coordinates.</param>
        /// <returns>The constructed ray.</returns>
        public Ray RayForScreenCoordinate(float x, float y)
        {
            var right = new Vector3(this.transform.M11, this.transform.M12, this.transform.M13);
            var up = new Vector3(this.transform.M21, this.transform.M22, this.transform.M23);
            var forward = new Vector3(this.transform.M31, this.transform.M32, this.transform.M33);
            var origin = new Vector3(this.transform.M41, this.transform.M42, this.transform.M43);

            var direction = Vector3.Normalize(forward + (right * (x * this.w)) + (up * (y * this.h)));
            return new Ray(origin, direction);
        }

        /// <summary>
        /// Create a ray from the camera that passes through the screen door at the center of the specified pixel.
        /// </summary>
        /// <remarks>
        /// The pixel coordinate <paramref name="x"/> is assumed to be numbered from 0 to
        /// cols-1 inclusive. Similarly, the pixel coordinate <paramref name="y"/> is assumed to
        /// be numbered from 0 through to rows-1 inclusive. The pixel coordinate (0, 0) is assumed
        /// to be the top left corner of the screen.
        /// </remarks>
        /// <param name="c">The column for the pixel, in the interval [0, cols-1]</param>
        /// <param name="r">The row for the pixel, in the interval [0, rows-1]</param>
        /// <param name="cols">The total number of columns of pixels.</param>
        /// <param name="rows">The total number of rows of pixels.</param>
        /// <returns>The constructed ray.</returns>
        public Ray RayForPixel(int c, int r, uint cols, uint rows)
        {
            var x = (((2.0F * c + 1.0F) / cols) - 1.0F);
            var y = -(((2.0F * r + 1.0F) / rows) - 1.0F);
            return RayForScreenCoordinate(x, y);
        }

        public Ray RayForSample(float c, float r, uint cols, uint rows)
        {
            var x = (((2.0F * c) / cols) - 1.0F);
            var y = -(((2.0F * r) / rows) - 1.0F);
            return RayForScreenCoordinate(x, y);
        }
    }
}
