using System.Numerics;

namespace Raytracer
{
    public struct Plane
    {
        public readonly Vector3 Origin;
        public readonly Vector3 Normal;
        public readonly Color Color;

        public Plane(Vector3 origin, Vector3 normal, Color color)
        {
            Origin = origin;
            Normal = normal;
            Color = color;
        }

        public Plane(Vector3 origin, Vector3 normal)
        {
            Origin = origin;
            Normal = normal;
            Color = Color.White;
        }
    }
}
