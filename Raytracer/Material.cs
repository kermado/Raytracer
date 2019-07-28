using System;
using System.Numerics;

namespace Raytracer
{
    public enum TextureMapType
    {
        /// <summary>
        /// No texture.
        /// </summary>
        None,

        /// <summary>
        /// Spherical texture mapping.
        /// </summary>
        Spherical
    }

    public readonly struct Material
    {
        public readonly Color AmbientColor;
        public readonly Color DiffuseColor;
        public readonly Color SpecularColor;
        public readonly float Albedo;
        public readonly float Shininess;
        public readonly float Reflectivity;
        public readonly float Transparency;
        public readonly float RefractiveIndex;

        public readonly Vector2 Tiling;
        public readonly Texture DiffuseMap;
        public readonly Texture NormalMap;

        public Material(Color ambientColor, Color diffuseColor, Color specularColor, float albedo, float shininess,
            float reflectivity, float transparency, float refractiveIndex, Vector2 tiling, Texture diffuseMap, Texture normalMap)
        {
            AmbientColor = ambientColor;
            DiffuseColor = diffuseColor;
            SpecularColor = specularColor;
            Albedo = albedo;
            Shininess = shininess;
            Reflectivity = reflectivity;
            Transparency = transparency;
            RefractiveIndex = refractiveIndex;
            Tiling = tiling;
            DiffuseMap = diffuseMap;
            NormalMap = normalMap;
        }

        /// <summary>
        /// Looks up the normal in the tangent-space for the specified uv coordinates from the normal map.
        /// </summary>
        /// <param name="uv">The uv coordinates.</param>
        /// <returns>The surface normal.</returns>
        public Vector3 TangentSpaceNormal(in Vector2 uv)
        {
            // R [0,1] -> X [-1,1] (tangent)
            // G [0,1] -> Y [-1,1] (bitangent)
            // B [0,1] -> Z [0,1]  (normal)
            var color = NormalMap.BilinearFilteredColor(uv, Tiling);
            return Vector3.Normalize(new Vector3(2.0F * color.R - 1.0F, -2.0F * color.G + 1.0F, 2.0F * color.B - 1.0F));
        }

        /// <summary>
        /// Evaluates the Lambertian diffuse BRDF model.
        /// </summary>
        /// <param name="lightDirection">The light direction.</param>
        /// <param name="surfaceNormal">The surface normal.</param>
        /// <returns>The diffuse surface color.</returns>
        public Color DiffuseBRDF(in Vector3 lightDirection, in Vector3 surfaceNormal, in Vector2 uv)
        {
            var diffuseColor = (DiffuseMap != null) ? DiffuseMap.BilinearFilteredColor(uv, Tiling) : DiffuseColor;
            return diffuseColor * (Albedo / (float)Math.PI) * Math.Max(0.0F, Vector3.Dot(lightDirection, surfaceNormal));
        }

        /// <summary>
        /// Evaluates the Blinn-Phong specular BRDF model.
        /// </summary>
        /// <param name="cameraDirection">The camera direction.</param>
        /// <param name="lightDirection">The light direction.</param>
        /// <param name="surfaceNormal">The surface normal.</param>
        /// <returns>The specular surface color.</returns>
        public Color SpecularBRDF(in Vector3 cameraDirection, in Vector3 lightDirection, in Vector3 surfaceNormal)
        {
            var halfVector = Vector3.Normalize(cameraDirection + lightDirection);
            return SpecularColor * (float)Math.Pow(Math.Max(0.0F, Vector3.Dot(halfVector, surfaceNormal)), Shininess);
        }

        public static readonly Material Default = new Material(new Color(0.005F, 0.005F, 0.005F), new Color(0.6F, 0.6F, 0.6F), new Color(1.0F, 1.0F, 1.0F), 0.5F, 25.0F, 0.0F, 0.0F, 1.0F, Vector2.One, null, null);
        public static readonly Material Red = new Material(new Color(0.005F, 0.0F, 0.0F), Color.Red, Color.Black, 0.5F, 0.0F, 0.0F, 0.0F, 1.0F, Vector2.One, null, null);
        public static readonly Material Green = new Material(new Color(0.0F, 0.005F, 0.0F), Color.Green, Color.Black, 0.5F, 0.0F, 0.0F, 0.0F, 1.0F, Vector2.One, null, null);
        public static readonly Material Blue = new Material(new Color(0.0F, 0.0F, 0.005F), Color.Blue, Color.Black, 0.5F, 0.0F, 0.0F, 0.0F, 1.0F, Vector2.One, null, null);
        public static readonly Material Mirror = new Material(Color.Black, Color.White, Color.White, 0.01F, 5000.0F, 0.95F, 0.0F, 1.0F, Vector2.One, null, null);
        public static readonly Material Glass = new Material(Color.Black, Color.White, Color.Black, 0.0F, 0.0F, 0.0F, 1.0F, 1.52F, Vector2.One, null, null);
        public static readonly Material Checkerboard = new Material(new Color(0.005F, 0.005F, 0.005F), Color.White, Color.Black, 1.0F, 0.0F, 0.1F, 0.0F, 1.0F, Vector2.One, Texture.Checkerboard(2, 2, 1000, Color.White, Color.Black), null);
    }
}
