namespace Raytracer
{
    public struct Material
    {
        public readonly Color Ambient;
        public readonly Color Diffuse;
        public readonly Color Specular;
        public readonly float Shininess;
        public readonly float Reflectivity;

        public Material(Color ambient, Color diffuse, Color specular, float shininess, float reflectivity)
        {
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
            Shininess = shininess;
            Reflectivity = reflectivity;
        }

        public static readonly Material Default = new Material(new Color(0.005F, 0.005F, 0.005F), new Color(0.6F, 0.6F, 0.6F), new Color(1.0F, 1.0F, 1.0F), 25.0F, 0.0F);
    }
}
