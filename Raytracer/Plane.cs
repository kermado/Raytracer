using System.Numerics;

namespace Raytracer
{
    public readonly struct Plane
    {
        public readonly Vector3 Origin;
        public readonly Vector3 Normal;
        public readonly Vector3 FirstAxis;
        public readonly Material Material;

        public Plane(in Vector3 origin, in Vector3 normal, in Vector3 firstAxis, in Material material)
        {
            Origin = origin;
            Normal = normal;
            FirstAxis = firstAxis;
            Material = material;
        }

        public Plane(in Vector3 origin, in Vector3 normal, in Vector3 firstAxis)
        {
            Origin = origin;
            Normal = normal;
            FirstAxis = firstAxis;
            Material = Material.Default;
        }

        public Vector3 SecondAxis()
        {
            return Vector3.Normalize(Vector3.Cross(FirstAxis, Normal));
        }

        public Vector3 ReflectiveNormal(in Vector3 incidentDirection)
        {
            if (Vector3.Dot(Normal, incidentDirection) <= 0.0F)
            {
                return Normal;
            }

            return -Normal;
        }

        public Vector2 PlanarCoordinates(in Vector3 point)
        {
            var v = point - Origin;
            return new Vector2(Vector3.Dot(v, FirstAxis), Vector3.Dot(v, SecondAxis()));
        }
    }
}
