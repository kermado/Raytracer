using System.Numerics;

namespace Raytracer
{
    public struct Intersection
    {
        public readonly Ray Ray;
        public readonly float Distance;
        public readonly Vector3 Normal;
        public readonly Material Material;

        public Intersection(Ray ray, float distance, Vector3 normal, Material material)
        {
            Ray = ray;
            Distance = distance;
            Normal = normal;
            Material = material;
        }

        public Vector3 Point()
        {
            return Ray.Start + Ray.Direction * Distance;
        }

        public float DistanceSq()
        {
            return Distance * Distance;
        }
    }
}
