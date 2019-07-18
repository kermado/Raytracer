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

                if (numIntersections > 0)
                {
                    var point = Ray.Point(ray, dist1);
                    var normal = Vector3.Normalize(point - sphere.Center);
                    intersection = new Intersection(ray, dist1, normal, sphere.Color);
                    return true;
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
            const float bias = 0.0001F; // Bias to prevent self-shadowing (shadow acne).

            // Ray from camera.
            var intersection = new Intersection();
            if (Intersect(camera.RayForPixel(px, py, pw, ph), ref intersection))
            {
                // Point of intersection.
                var intersectionPoint = intersection.Point();
                var startPoint = intersectionPoint + intersection.Normal * bias;

                // Direct lighting.
                var color = Color.Black;
                foreach (var light in this.pointLights)
                {
                    var vecToLight = light.Position - startPoint;
                    var dirToLight = Vector3.Normalize(vecToLight);
                    if (Intersect(new Ray(startPoint, dirToLight), ref intersection) == false)
                    {
                        // If we hit something on the way to the light then we're in shadow.
                        // Otherwise, we have visibility from the light.
                        var intensity = light.Intensity / (4.0F * (float)Math.PI * vecToLight.LengthSquared()); // Inverse square law.
                        intensity *= Vector3.Dot(intersection.Normal, dirToLight); // Surface effect.
                        color += light.Color * intersection.Color * intensity;
                    }
                }

                return Color.Clamp(color);
            }

            return this.background;
        }
    }
}
