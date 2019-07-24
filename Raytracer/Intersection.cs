using System.Numerics;

namespace Raytracer
{
    public readonly struct Intersection
    {
        public readonly Ray Ray;
        public readonly float Distance;
        public readonly Vector3 Normal;
        public readonly Vector3 Tangent;
        public readonly Vector3 Bitangent;
        public readonly Vector2 UV;
        public readonly Material Material;

        public Intersection(Ray ray, float distance, Vector3 normal, Vector3 tangent, Vector3 bitangent, Vector2 uv, Material material)
        {
            Ray = ray;
            Distance = distance;
            Normal = normal;
            Tangent = tangent;
            Bitangent = bitangent;
            UV = uv;
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
