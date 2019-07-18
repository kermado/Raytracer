using System.Numerics;

namespace Raytracer
{
    public struct Intersection
    {
        public readonly Ray Ray;
        public readonly float Distance;
        public readonly Vector3 Normal;

        public Intersection(Ray ray, float distance, Vector3 normal)
        {
            Ray = ray;
            Distance = distance;
            Normal = normal;
        }

        public Vector3 Point()
        {
            return Ray.Start + Ray.Direction * Distance;
        }
    }
}
