using System;
using System.Numerics;

namespace Raytracer
{
    public struct PointLight
    {
        public readonly Vector3 Position;
        public readonly Color Color;
        public readonly float Intensity;

        public PointLight(Vector3 position, Color color, float intensity)
        {
            Position = position;
            Color = color;
            Intensity = intensity;
        }
    }
}
