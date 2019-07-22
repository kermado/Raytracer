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
        /// <param name="distance">The distance along the ray at which the first point of intersection occurs.</param>
        /// <returns>Whether the ray intersects the sphere.</returns>
        public static bool RaySphere(in Ray ray, in Sphere sphere, out float distance)
        {
            // Transform the start of the ray to the sphere's local coordinate system, where the
            // sphere's center is positioned at the origin.
            var sr = ray.Start - sphere.Center; // ray start
            var dr = ray.Direction;
            var rs = sphere.Radius;

            // Check whether the ray intersects the sphere.
            // We can do this more efficiently than solving a quadratic.
            var rsrs = rs * rs;
            var srsr = sr.LengthSquared();
            var drdr = dr.LengthSquared();
            var srdr = Vector3.Dot(sr, dr);
            var srdrsq = srdr * srdr;
            if (srsr - 2.0F * srdrsq + drdr * srdrsq > rsrs)
            {
                distance = 0.0F;
                return false;
            }

#if FAST_RAY_SPHERE
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
            float distance2;
            switch (SolveQuadratic(1.0F, 2.0F * srdr, srsr - rs, out distance, out distance2))
            {
                case 1: return Positive(distance);
                default: return SmallestPositive(distance, distance2, out distance);
            }
#else
            // This method is slower but more precise and can help eliminate shadow acne for cases where the sphere is
            // far away (i.e. sr is large). In practice, I have found that it isn't much more expensive than the trivial
            // method above and so it is used by default. This method is descrived in Ray Tracing Gems by Eric Haines
            // and Tomas Akenine-Moller.
            var bprime = -srdr;
            var discriminant = rsrs - (sr + (bprime / drdr) * dr).LengthSquared();
            if (discriminant > 0.0F)
            {
                var c = srsr - rsrs;
                var q = bprime + (float)Math.Sign(bprime) * (float)Math.Sqrt(drdr * discriminant);
                return SmallestPositive(c / q, q / drdr, out distance);
            }
            else
            {
                distance = 0.0F;
                return false;
            }
#endif
        }

        /// <summary>
        /// Intersects a ray with a plane.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="plane">The plane.</param>
        /// <param name="distance">The distance along the ray at which the point of intersection occurs.</param>
        /// <returns>Whether the ray intersects the plane.</returns>
        public static bool RayPlane(in Ray ray, in Plane plane, out float distance)
        {
            // A point on a plane is parameterized by (p - o).n = 0, where n is the normal to the
            // plane and o is a point on the plane.
            // A point on a ray is parameterized by p = s + d*l, where s is the start of the ray and
            // d is the direction of the ray.
            //
            // Intersection occurs when ((s + d*l) - o).n = 0 <==> s.n + l(d.n) - o.n = 0
            //                                                <==> l = (o.n - s.n) / d.n
            //
            // If d.n = 0 then the ray is perpendicular to the plane. We consider the ray to not
            // intersect the plane in such a case.
            var op = plane.Origin;
            var np = plane.Normal;
            var or = ray.Start;
            var dr = ray.Direction;

            float denom = Vector3.Dot(dr, np);
            if (denom != 0.0F)
            {
                distance = (Vector3.Dot(op, np) - Vector3.Dot(or, np)) / denom;
                return distance > 0.0F;
            }

            distance = 0.0F;
            return false;
        }

        /// <summary>
        /// Whether the value specified is strictly positive.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>Whether the value is strictly positive.</returns>
        private static bool Positive(float val)
        {
            return val > 0.0F;
        }

        /// <summary>
        /// Whether one or more of the input values are strictly positive. The output value is set
        /// to the smallest strictly positive input value.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <param name="output">The output value.</param>
        /// <returns>The number of positive values.</returns>
        private static bool SmallestPositive(float val1, float val2, out float output)
        {
            if (val1 > 0.0F && val2 > 0.0F)
            {
                output = Math.Min(val1, val2);
                return true;
            }
            else if (val1 > 0.0F)
            {
                output = val1;
                return true;
            }
            else if (val2 > 0.0F)
            {
                output = val2;
                return true;
            }

            output = 0.0F;
            return false;
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
