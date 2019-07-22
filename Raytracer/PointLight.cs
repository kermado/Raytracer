using System;
using System.Numerics;

namespace Raytracer
{
    public readonly struct PointLight
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

        public Color ColorIntensity(float distanceSq)
        {
            return Color * (Intensity / (4.0F * (float)Math.PI * distanceSq));
        }
    }
}
