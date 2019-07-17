using System.Numerics;

namespace Raytracer
{
    public struct Ray
    {
        public readonly Vector3 Start;
        public readonly Vector3 Direction;

        public Ray(Vector3 start, Vector3 direction)
        {
            Start = start;
            Direction = direction;
        }

        public static Vector3 Point(Ray ray, float length)
        {
            return ray.Start + ray.Direction * length;
        }
    }
}
