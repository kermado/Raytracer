using System;
using System.Collections.Generic;
using System.Numerics;

namespace Raytracer
{
    public sealed class Scene
    {
        /// <summary>
        /// The background color for the scene.
        /// </summary>
        private Color background;

        /// <summary>
        /// The list of point light sources in the scene.
        /// </summary>
        private List<PointLight> pointLights;

        /// <summary>
        /// The list of sphere primitives in the scene.
        /// </summary>
        private List<Sphere> spheres;

        /// <summary>
        /// The background color for the scene.
        /// </summary>
        public Color Background
        {
            get { return this.background; }
            set { this.background = value; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Scene()
        {
            this.pointLights = new List<PointLight>();
            this.spheres = new List<Sphere>();
        }

        /// <summary>
        /// Adds a point light source to the scene.
        /// </summary>
        /// <param name="light">The point light.</param>
        public void Add(PointLight light)
        {
            this.pointLights.Add(light);
        }

        /// <summary>
        /// Adds a sphere to the scene.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        public void Add(Sphere sphere)
        {
            this.spheres.Add(sphere);
        }

        /// <summary>
        /// Tests for intersections between the provided ray and every object in the scene.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="intersection">The intersection for the nearest object.</param>
        /// <returns>Whether the ray intersected an object.</returns>
        public bool Intersect(Ray ray, ref Intersection intersection)
        {
            // TODO: Handle intersections with multiple shapes (depth ordering).
            foreach (var sphere in this.spheres)
            {
                var numIntersections = Raytracer.Intersect.RaySphere(ray, sphere, out var dist1, out var dist2);
                switch (numIntersections)
                {
                    case 0: return false;
                    case 1: intersection = new Intersection(ray, dist1); return true;
                    case 2: intersection = new Intersection(ray, Math.Min(dist1, dist2)); return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines the color for the specified pixel.
        /// </summary>
        /// <param name="camera">The camera.</param>
        /// <param name="px">The horizontal pixel coordinate, in the interval [0, pw-1].</param>
        /// <param name="py">The vertical pixel coordinate, in the interval [0, ph-1].</param>
        /// <param name="pw">The total number of horizontal pixels.</param>
        /// <param name="ph">The total number of vertical pixels.</param>
        /// <returns>The color for the pixel.</returns>
        public Color PixelColor(PerspectiveCamera camera, int px, int py, int pw, int ph)
        {
            // Ray from camera.
            var intersection = new Intersection();
            if (Intersect(camera.RayForPixel(px, py, pw, ph), ref intersection))
            {
                // Point of intersection.
                var intersectionPoint = intersection.Point();

                // Direct lighting.
                foreach (var light in this.pointLights)
                {
                    var dirToLight = Vector3.Normalize(light.Position - intersectionPoint);
                    if (Intersect(new Ray(intersectionPoint, dirToLight), ref intersection))
                    {
                        // If we hit something on the way to the light then we're in shadow.
                        return Color.Black;
                    }
                    else
                    {
                        // Full visibility from the light.
                        return Color.White;
                    }
                }
            }

            return this.background;
        }
    }
}
