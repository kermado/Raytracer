using System;
using System.Diagnostics;
using System.Numerics;

namespace Raytracer
{
    public static class Intersect
    {
        /// <summary>
        /// Intersects a ray with a sphere.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="sphere">The sphere.</param>
        /// <param name="l1">The distance along the ray at which the first point of intersection occurs.</param>
        /// <param name="l2">The distance along the ray at which the second point of intersection occurs.</param>
        /// <returns>The number of points of intersection, in the interval [0, 2].</returns>
        public static int RaySphere(Ray ray, Sphere sphere, out float l1, out float l2)
        {
            // Transform the start of the ray to the sphere's local coordinate system, where the
            // sphere's center is positioned at the origin.
            var sr = ray.Start - sphere.Center; // ray start
            var dr = ray.Direction;
            var rs = sphere.Radius;

            // A point on the sphere is parameterized by ||p|| = r, where r is the radius.
            // A point on a ray is parameterized by p = s + d*l, where s is the start of the ray,
            // d is the unit vector direction of the ray and l is some length along the ray.
            //
            // Intersection occurs when ||s + d*l|| = r <==> ||s + d*l||^2 = r^2
            //                                          <==> (d.d)*l^2 + 2l*(s.d) + s.s - r^2
            //
            // However, ||d|| = 1, so d.d = 1.
            //
            // We solve this quadratic for l, the length along the ray at the points of
            // intersection.
            int count = SolveQuadratic(1.0F, 2.0F * Vector3.Dot(sr, dr), Vector3.Dot(sr, sr) - rs, out l1, out l2);
            switch (count)
            {
                case 1: return Positive(ref l1);
                case 2: return PositiveOrdered(ref l1, ref l2);
            }

            return 0;
        }

        /// <summary>
        /// Returns 1 if the provided value is positive, otherwise returns 0.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>Whether the value is positive.</returns>
        private static int Positive(ref float val)
        {
            if (val > 0.0F)
            {
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Returns the number of positive values and orders the values so that the positive values
        /// come first, ordered by size.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <returns>The number of positive values.</returns>
        private static int PositiveOrdered(ref float val1, ref float val2)
        {
            if (val1 > 0.0F && val2 > 0.0F)
            {
                if (val1 > val2)
                {
                    float temp = val1;
                    val1 = val2;
                    val2 = temp;
                }

                return 2;
            }
            else if (val1 > 0.0F)
            {
                return 1;
            }
            else if (val2 > 0.0F)
            {
                float temp = val1;
                val1 = val2;
                val2 = temp;

                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Solves a quadratic equation.
        /// </summary>
        /// <param name="a">The first coefficient, for the x^2 term (must be non-zero).</param>
        /// <param name="b">The second coefficient, for the x term.</param>
        /// <param name="c">The third coefficient, for the constant term.</param>
        /// <param name="r1">The first real solution.</param>
        /// <param name="r2">The second real solution.</param>
        /// <returns>The number of real solutions.</returns>
        private static int SolveQuadratic(float a, float b, float c, out float r1, out float r2)
        {
            Debug.Assert(a != 0.0F); // Non-quadratic.

            var discriminant = b * b - 4.0F * a * c;
            if (discriminant > 0.0F)
            {
                var sqrt = (float)Math.Sqrt(discriminant);
                var denom = 1.0F / (2.0F * a);
                r1 = (-b + sqrt) * denom;
                r2 = (-b - sqrt) * denom;
                return 2;
            }
            else if (discriminant == 0.0F)
            {
                r1 = -b / (2.0F * a);
                r2 = 0.0F;
                return 1;
            }

            r1 = 0.0F;
            r2 = 0.0F;
            return 0;
        }
    }
}
