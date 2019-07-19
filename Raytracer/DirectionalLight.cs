using System.Numerics;

namespace Raytracer
{
    public struct DirectionalLight
    {
        public readonly Vector3 Direction;
        public readonly Color Color;
        public readonly float Intensity;

        public DirectionalLight(Vector3 direction, Color color, float intensity)
        {
            Direction = direction;
            Color = color;
            Intensity = intensity;
        }
    }
}
