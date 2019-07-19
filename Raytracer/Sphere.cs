using System.Numerics;

namespace Raytracer
{
    public struct Sphere
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
    }
}
