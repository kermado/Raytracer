using System.Numerics;

namespace Raytracer
{
    public readonly struct Ray
    {
        public readonly Vector3 Start;
        public readonly Vector3 Direction;

        public Ray(Vector3 start, Vector3 direction)
        {
            Start = start;
            Direction = direction;
        }

        public Vector3 Point(float length)
        {
            return Start + Direction * length;
        }
    }
}
