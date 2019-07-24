using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace Raytracer
{
    public sealed class Scene
    {
        private enum ShapeType
        {
            None,
            Sphere,
            Plane
        }

        private static readonly float bias = 0.0001F;

        /// <summary>
        /// The background color for the scene.
        /// </summary>
        private Color background;

        /// <summary>
        /// The list of point light sources in the scene.
        /// </summary>
        private List<PointLight> pointLights;

        /// <summary>
        /// The list of directional light sources in the scene.
        /// </summary>
        private List<DirectionalLight> directionalLights;

        /// <summary>
        /// The list of sphere primitives in the scene.
        /// </summary>
        private List<Sphere> spheres;

        /// <summary>
        /// The list of plane primitives in the scene.
        /// </summary>
        private List<Plane> planes;

        /// <summary>
        /// The background color for the scene.
        /// </summary>
        public Color Background
        {
            get { return this.background; }
            set { this.background = value; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Scene()
        {
            this.background = Color.Black;
            this.pointLights = new List<PointLight>();
            this.directionalLights = new List<DirectionalLight>();
            this.spheres = new List<Sphere>();
            this.planes = new List<Plane>();
        }

        /// <summary>
        /// Adds a point light source to the scene.
        /// </summary>
        /// <param name="light">The point light.</param>
        public void Add(in PointLight light)
        {
            this.pointLights.Add(light);
        }

        /// <summary>
        /// Adds a directional light source to the scene.
        /// </summary>
        /// <param name="light">The directional light.</param>
        public void Add(in DirectionalLight light)
        {
            this.directionalLights.Add(light);
        }

        /// <summary>
        /// Adds a sphere to the scene.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        public void Add(in Sphere sphere)
        {
            this.spheres.Add(sphere);
        }

        /// <summary>
        /// Adds a plane to the scene.
        /// </summary>
        /// <param name="plane">The plane.</param>
        public void Add(in Plane plane)
        {
            this.planes.Add(plane);
        }

        /// <summary>
        /// Tests for intersections between the provided ray and every object in the scene.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="intersection">The intersection for the nearest object.</param>
        /// <returns>Whether the ray intersected an object.</returns>
        private bool Intersect(in Ray ray, ref Intersection intersection)
        {
            float minDistance = float.MaxValue;
            ShapeType minShapeType = ShapeType.None;
            int minIndex = 0;

            for (int i = 0; i < this.spheres.Count; ++i)
            {
                if (Raytracer.Intersect.RaySphere(ray, this.spheres[i], out var distance))
                {
                    Debug.Assert(distance > 0.0F);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        minShapeType = ShapeType.Sphere;
                        minIndex = i;
                    }
                }
            }

            for (int i = 0; i < this.planes.Count; ++i)
            {
                if (Raytracer.Intersect.RayPlane(ray, this.planes[i], out var distance))
                {
                    Debug.Assert(distance > 0.0F);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        minShapeType = ShapeType.Plane;
                        minIndex = i;
                    }
                }
            }

            // Did we hit anything?
            if (minShapeType != ShapeType.None)
            {
                Vector3 point = Ray.Point(ray, minDistance);
                Vector3 normal;
                Vector3 tangent;
                Vector3 bitangent;
                Vector2 uv;
                switch (minShapeType)
                {
                    case ShapeType.Sphere:
                        normal = this.spheres[minIndex].ReflectiveNormal(point, ray.Direction);
                        tangent = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, normal)); // TODO: Simplify this.
                        bitangent = Vector3.Cross(normal, tangent); // No need to normalize, since normal and tangent are orthogonal.
                        uv = TextureMapping.Spherical(Vector3.Normalize(point - this.spheres[minIndex].Center));
                        intersection = new Intersection(ray, minDistance, normal, tangent, bitangent, uv, this.spheres[minIndex].Material);
                        break;
                    case ShapeType.Plane:
                        normal = this.planes[minIndex].ReflectiveNormal(ray.Direction);
                        tangent = this.planes[minIndex].FirstAxis;
                        bitangent = this.planes[minIndex].SecondAxis();
                        uv = this.planes[minIndex].PlanarCoordinates(point);
                        intersection = new Intersection(ray, minDistance, normal, tangent, bitangent, uv, this.planes[minIndex].Material);
                        break;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines the transmission direction due to refraction.
        /// </summary>
        /// <param name="incidentDirection">The incident direction of the ray hitting the surface.</param>
        /// <param name="surfaceNormal">The normal at the surface point.</param>
        /// <param name="refractiveIndex">The refractive index of the material.</param>
        /// <param name="transmissionDirection">The transmission direction.</param>
        /// <returns>True if transmission is possible, false if there is total internal reflection.</returns>
        private bool Refract(Vector3 incidentDirection, Vector3 surfaceNormal, float refractiveIndex, out Vector3 transmissionDirection)
        {
            float cosi = Math.Min(1.0F, Math.Max(-1.0F, Vector3.Dot(incidentDirection, surfaceNormal)));
            float fior = 1.0F; // Refractive index of the first medium.
            float sior = refractiveIndex; // Refractive index of the second medium.

            if (cosi < 0.0F) // Ray goes from first medium into second medium.
            {
                cosi = -cosi;
            }
            else // Ray goes from second medium into first medium.
            {
                float temp = fior;
                fior = sior;
                sior = temp;

                surfaceNormal = -surfaceNormal;
            }

            float ratio = fior / sior;
            float k = 1.0F - ratio * ratio * (1.0F - cosi * cosi);
            if (k > 0.0F)
            {
                transmissionDirection = (incidentDirection * ratio) + surfaceNormal * (ratio * cosi - (float)Math.Sqrt(k));
                return true;
            }

            // If k < 0 then we have total internal reflection.
            // If k = 0 then the refracted ray is parallel to the surface normal.
            transmissionDirection = Vector3.Zero;
            return false;
        }

        /// <summary>
        /// Casts a sequence of shadow rays to the light and determines the transmittance.
        /// </summary>
        /// <remarks>
        /// The transmittance will be 1 if there is a completely unimpeeded path from the surface point to the light.
        /// Conversely, the transmittance will be 0 if the path from the surface point to the light is completely
        /// blocked. The transmittance will be somewhere inbetween if there is a sequence of transparent objects between
        /// the surface point and the light.
        /// </remarks>
        /// <param name="biasedSurfacePoint">The biased point on the surface of the object.</param>
        /// <param name="lightDirection">The direction from the surface point to the light.</param>
        /// <param name="lightDistance">The distance from the surface point to the light.</param>
        /// <returns>The transmittance to the light.</returns>
        private float Transmittance(in Vector3 biasedSurfacePoint, in Vector3 lightDirection, float lightDistance)
        {
            var intersection = new Intersection();

            var rayStart = biasedSurfacePoint;

            float transmittance = 1.0F;
            while (transmittance > 0.0F)
            {
                if (Intersect(new Ray(rayStart, lightDirection), ref intersection))
                {
                    if (intersection.Distance < lightDistance)
                    {
                        transmittance *= intersection.Material.Transparency;
                    }
                    else
                    {
                        // We're already beyond the light, so no point in continuing.
                        break;
                    }

                    // We must start again from the point that we hit.
                    // Note that the intersection normal is the reflective normal and so we must bias the point on the
                    // surface by the negative of that normal in order to travel through the surface.
                    rayStart = intersection.Point() - intersection.Normal * bias;
                    lightDistance -= intersection.Distance;
                }
                else
                {
                    // No intersection, so we are done.
                    break;
                }
            }

            return transmittance;
        }

        /// <summary>
        /// Traces the specified view ray, reflecting up to <paramref name="maxDepth"/> times.
        /// </summary>
        /// <param name="viewRay">The view ray.</param>
        /// <param name="depth">The maximum number of reflections.</param>
        /// <returns>The resulting color</returns>
        public Color Trace(in Ray viewRay, int depth, int maxDepth)
        {
            var color = Color.Black;

            // Visibility.
            var viewIntersection = new Intersection();
            if (Intersect(viewRay, ref viewIntersection))
            {
                var surfacePoint = viewIntersection.Point();
                var surfaceNormal = viewIntersection.Normal;
                var surfaceMaterial = viewIntersection.Material;
                var surfaceUV = viewIntersection.UV;

                // We must bias the surface point in order to prevent self-shadowing.
                var biasedSurfacePoint = surfacePoint + surfaceNormal * bias;

                // Shadows.
                foreach (var light in this.pointLights)
                {
                    var lightVector = light.Position - biasedSurfacePoint;
                    var lightDistance = lightVector.Length();
                    var lightDirection = lightVector / lightDistance;
                    var transmittance = Transmittance(biasedSurfacePoint, lightDirection, lightDistance);
                    if (transmittance > 0.0F)
                    {
                        var lightColorIntensity = light.ColorIntensity(lightDistance * lightDistance) * transmittance;
                        var diffuse = surfaceMaterial.DiffuseBRDF(lightDirection, surfaceNormal, surfaceUV);
                        var specular = surfaceMaterial.SpecularBRDF(-viewRay.Direction, lightDirection, surfaceNormal);
                        color += lightColorIntensity * (diffuse + specular);
                    }
                }
                
                foreach (var light in this.directionalLights)
                {
                    var lightDirection = -light.Direction;
                    var transmittance = Transmittance(biasedSurfacePoint, lightDirection, float.PositiveInfinity);
                    if (transmittance > 0.0F)
                    {
                        var lightColorIntensity = light.Color * light.Intensity * transmittance;
                        var diffuse = surfaceMaterial.DiffuseBRDF(lightDirection, surfaceNormal, surfaceUV);
                        var specular = surfaceMaterial.SpecularBRDF(-viewRay.Direction, lightDirection, surfaceNormal);
                        color += lightColorIntensity * (diffuse + specular);
                    }
                }

                // Recursion.
                if (depth < maxDepth)
                {
                    // Reflection.
                    if (surfaceMaterial.Reflectivity > 0.0F)
                    {
                        color += Trace(new Ray(biasedSurfacePoint, Vector3.Reflect(viewRay.Direction, surfaceNormal)), depth + 1, maxDepth) * surfaceMaterial.Reflectivity;
                    }

                    // Refraction.
                    if (surfaceMaterial.Transparency > 0.0F)
                    {
                        if (Refract(viewRay.Direction, surfaceNormal, surfaceMaterial.RefractiveIndex, out var transmissionDirection))
                        {
                            var biasedRefractiveSurfacePoint = surfacePoint + transmissionDirection * bias;
                            color += Trace(new Ray(biasedRefractiveSurfacePoint, transmissionDirection), depth + 1, maxDepth) * surfaceMaterial.Transparency;
                        }
                    }
                }
            }
            else
            {
                color = this.background;
            }

            return color;
        }

        /// <summary>
        /// Determines the color for the specified pixel.
        /// </summary>
        /// <param name="camera">The camera.</param>
        /// <param name="px">The horizontal pixel coordinate, in the interval [0, pw-1].</param>
        /// <param name="py">The vertical pixel coordinate, in the interval [0, ph-1].</param>
        /// <param name="pw">The total number of horizontal pixels.</param>
        /// <param name="ph">The total number of vertical pixels.</param>
        /// <returns>The color for the pixel.</returns>
        public Color PixelColor(PerspectiveCamera camera, int px, int py, int pw, int ph)
        {
            var color = Trace(camera.RayForPixel(px, py, pw, ph), 0, 10);
            return Color.Clamp(Color.CorrectGamma(color, camera.Exposure, 1.0F / camera.Gamma));
        }
    }
}
