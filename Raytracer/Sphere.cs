using System.Numerics;

namespace Raytracer
{
    public struct Sphere
    {
        public readonly Vector3 Center;
        public readonly float Radius;
        public readonly Color Color;

        public Sphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
            Color = Color.White;
        }

        public Sphere(Vector3 center, float radius, Color color)
        {
            Center = center;
            Radius = radius;
            Color = color;
        }
    }
}
