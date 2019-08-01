using System;
using System.Numerics;

namespace Raytracer
{
    public static class TextureMapping
    {
        /// <summary>
        /// Determines the uv coordinates for a sphere.
        /// </summary>
        /// <param name="direction">The direction from the origin of the sphere to the surface point.</param>
        /// <returns>The uv coordinates for the surface point.</returns>
        public static Vector2 Spherical(in Vector3 direction)
        {
            const float invpi = 1.0F / (float)Math.PI;
            float u = 0.5F + 0.5F * (float)Math.Atan2(direction.Z, direction.X) * invpi;
            float v = 0.5F - (float)Math.Asin(Math.Max(-1.0F, Math.Min(1.0F, direction.Y))) * invpi;
            return new Vector2(u, v);
        }

        public static Vector2 Planar(in Plane plane, in Vector3 point)
        {
            return plane.PlanarCoordinates(point);
        }
    }
}
