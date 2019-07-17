using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public sealed class Scene
    {
        /// <summary>
        /// The list of sphere primitives in the scene.
        /// </summary>
        private List<Sphere> spheres;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Scene()
        {
            this.spheres = new List<Sphere>();
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
    }
}
