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

        public Intersection(in Ray ray, float distance, in Vector3 normal, in Vector3 tangent, in Vector2 uv, in Material material)
        {
            Ray = ray;
            Distance = distance;
            Normal = normal;
            Tangent = tangent;
            Bitangent = Vector3.Cross(normal, tangent); // No need to normalize, since normal and tangent should be orthogonal.
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
