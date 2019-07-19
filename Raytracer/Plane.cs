using System.Numerics;

namespace Raytracer
{
    public struct Plane
    {
        public readonly Vector3 Origin;
        public readonly Vector3 Normal;
        public readonly Material Material;

        public Plane(Vector3 origin, Vector3 normal, Material material)
        {
            Origin = origin;
            Normal = normal;
            Material = material;
        }

        public Plane(Vector3 origin, Vector3 normal)
        {
            Origin = origin;
            Normal = normal;
            Material = Material.Default;
        }
    }
}
