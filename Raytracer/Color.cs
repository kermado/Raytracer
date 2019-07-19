using System;

namespace Raytracer
{
    public struct Color
    {
        public readonly float R;
        public readonly float G;
        public readonly float B;

        public Color(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public static Color Clamp(Color color)
        {
            return new Color((float)Math.Min(1.0F, color.R), (float)Math.Min(1.0F, color.G), (float)Math.Min(1.0F, color.B));
        }

        public static Color CorrectGamma(Color color, float exposure, float gamma)
        {
            return new Color((float)Math.Pow(exposure * color.R, gamma), (float)Math.Pow(exposure * color.G, gamma), (float)Math.Pow(exposure * color.B, gamma));
        }

        public static Color operator*(Color color, float scalar)
        {
            return new Color(color.R * scalar, color.G * scalar, color.B * scalar);
        }

        public static Color operator *(float scalar, Color color)
        {
            return color * scalar;
        }

        public static Color operator +(Color c1, Color c2)
        {
            return new Color(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B);
        }

        public static Color operator*(Color c1, Color c2)
        {
            return new Color(c1.R * c2.R, c1.G * c2.G, c1.B * c2.B);
        }

        public static readonly Color Black = new Color(0.0F, 0.0F, 0.0F);
        public static readonly Color Red = new Color(1.0F, 0.0F, 0.0F);
        public static readonly Color Green = new Color(0.0F, 1.0F, 0.0F);
        public static readonly Color Blue = new Color(0.0F, 0.0F, 1.0F);
        public static readonly Color White = new Color(1.0F, 1.0F, 1.0F);
    }
}
