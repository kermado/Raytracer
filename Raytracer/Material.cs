using System;
using System.Numerics;

namespace Raytracer
{
    public struct Material
    {
        public readonly Color Ambient;
        public readonly Color Diffuse;
        public readonly Color Specular;
        public readonly float Albedo;
        public readonly float Shininess;
        public readonly float Reflectivity;

        public Material(Color ambient, Color diffuse, Color specular, float albedo, float shininess, float reflectivity)
        {
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
            Albedo = albedo;
            Shininess = shininess;
            Reflectivity = reflectivity;
        }

        /// <summary>
        /// Evaluates the Lambertian diffuse BRDF model.
        /// </summary>
        /// <param name="lightDirection">The light direction.</param>
        /// <param name="surfaceNormal">The surface normal.</param>
        /// <returns>The diffuse surface color.</returns>
        public Color DiffuseBRDF(Vector3 lightDirection, Vector3 surfaceNormal)
        {
            return Diffuse * (Albedo / (float)Math.PI) * Math.Max(0.0F, Vector3.Dot(lightDirection, surfaceNormal));
        }

        /// <summary>
        /// Evaluates the Blinn-Phong specular BRDF model.
        /// </summary>
        /// <param name="cameraDirection">The camera direction.</param>
        /// <param name="lightDirection">The light direction.</param>
        /// <param name="surfaceNormal">The surface normal.</param>
        /// <returns>The specular surface color.</returns>
        public Color SpecularBRDF(Vector3 cameraDirection, Vector3 lightDirection, Vector3 surfaceNormal)
        {
            var halfVector = Vector3.Normalize(cameraDirection + lightDirection);
            return Specular * (float)Math.Pow(Math.Max(0.0F, Vector3.Dot(halfVector, surfaceNormal)), Shininess);
        }

        public static readonly Material Default = new Material(new Color(0.005F, 0.005F, 0.005F), new Color(0.6F, 0.6F, 0.6F), new Color(1.0F, 1.0F, 1.0F), 0.5F, 25.0F, 0.0F);
        public static readonly Material Red = new Material(new Color(0.005F, 0.0F, 0.0F), Color.Red, Color.White, 0.5F, 25.0F, 0.0F);
        public static readonly Material Green = new Material(new Color(0.0F, 0.005F, 0.0F), Color.Green, Color.White, 0.5F, 25.0F, 0.0F);
        public static readonly Material Blue = new Material(new Color(0.0F, 0.0F, 0.005F), Color.Blue, Color.White, 0.5F, 25.0F, 0.0F);
        public static readonly Material Mirror = new Material(Color.Black, Color.Black, Color.Black, 0.0F, 0.0F, 1.0F);
    }
}
