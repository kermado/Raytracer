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
        private float vfov;

        /// <summary>
        /// The aspect ratio (with/height).
        /// </summary>
        private float ar;

        /// <summary>
        /// The half-height of the screen door.
        /// </summary>
        private float h;

        /// <summary>
        /// The half-width of the screen door.
        /// </summary>
        private float w;

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
        /// Default constructor.
        /// </summary>
        public PerspectiveCamera()
        {
            this.transform = Matrix4x4.Identity;
            this.vfov = (float)(Math.PI * 0.25);
            this.ar = (float)(16.0 / 9.0);

            UpdateScreenDimensions();
        }

        /// <summary>
        /// Updates the half-width and half-height of the screen door.
        /// </summary>
        /// <remarks>
        /// Must be called after updating the vertical field-of-view or the aspect ratio.
        /// </remarks>
        private void UpdateScreenDimensions()
        {
            this.h = (float)Math.Tan(this.vfov);
            this.w = this.h * ar;
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
        /// Create a ray from the camera that passes through the screen door at the top left corner
        /// of the specified pixel.
        /// </summary>
        /// <remarks>
        /// The pixel coordinate <paramref name="x"/> is assumed to be numbered from 0 to
        /// cols-1 inclusive. Similarly, the pixel coordinate <paramref name="y"/> is assumed to
        /// be numbered from 0 through to rows-1 inclusive. The pixel coordinate (0, 0) is assumed
        /// to be the top left corner of the screen.
        /// 
        /// The ray constructed passes through the top left corner of the pixel. Whilst a more
        /// correct implementation would pass through the center of the pixel, using this definition
        /// is faster and will just result in the image being shifted up and to the left by just
        /// half a pixel.
        /// </remarks>
        /// <param name="c">The column for the pixel, in the interval [0, cols-1]</param>
        /// <param name="r">The row for the pixel, in the interval [0, rows-1]</param>
        /// <param name="cols">The total number of columns of pixels.</param>
        /// <param name="rows">The total number of rows of pixels.</param>
        /// <returns>The constructed ray.</returns>
        public Ray RayForPixel(int c, int r, int cols, int rows)
        {
            var x = (((2.0F * c) / cols) - 1.0F);
            var y = -(((2.0F * r) / rows) - 1.0F);
            return RayForScreenCoordinate(x, y);
        }
    }
}
