using System.Numerics;

namespace Raytracer
{
    public struct Intersection
    {
        public readonly Ray Ray;
        public readonly float Distance;
        public readonly Vector3 Normal;
        public readonly Color Color;

        public Intersection(Ray ray, float distance, Vector3 normal, Color color)
        {
            Ray = ray;
            Distance = distance;
            Normal = normal;
            Color = color;
        }

        public Vector3 Point()
        {
            return Ray.Start + Ray.Direction * Distance;
        }
    }
}
