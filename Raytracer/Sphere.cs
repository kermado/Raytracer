using System.Numerics;

namespace Raytracer
{
    public struct Sphere
    {
        public readonly Vector3 Center;
        public readonly float Radius;

        public Sphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
    }
}
