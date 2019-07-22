using System.Numerics;

namespace Raytracer
{
    public readonly struct Sphere
    {
        public readonly Vector3 Center;
        public readonly float Radius;
        public readonly Material Material;

        public Sphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
            Material = Material.Default;
        }

        public Sphere(Vector3 center, float radius, Material material)
        {
            Center = center;
            Radius = radius;
            Material = material;
        }

        public Vector3 ReflectiveNormal(in Vector3 surfacePoint, in Vector3 incidentDirection)
        {
            var normal = (surfacePoint - Center) / Radius;
            if (Vector3.Dot(incidentDirection, normal) <= 0.0F)
            {
                return normal;
            }

            return -normal;
        }
    }
}
