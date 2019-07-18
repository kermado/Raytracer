namespace Raytracer
{
    public struct Color
    {
        public readonly float R;
        public readonly float G;
        public readonly float B;
        public readonly float A;

        public Color(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
            A = 1.0F;
        }

        public static readonly Color Black = new Color(0.0F, 0.0F, 0.0F, 1.0F);
        public static readonly Color White = new Color(1.0F, 1.0F, 1.0F, 1.0F);
    }
}
