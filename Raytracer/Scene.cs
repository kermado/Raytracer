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
        public void Add(PointLight light)
        {
            this.pointLights.Add(light);
        }

        /// <summary>
        /// Adds a directional light source to the scene.
        /// </summary>
        /// <param name="light">The directional light.</param>
        public void Add(DirectionalLight light)
        {
            this.directionalLights.Add(light);
        }

        /// <summary>
        /// Adds a sphere to the scene.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        public void Add(Sphere sphere)
        {
            this.spheres.Add(sphere);
        }

        /// <summary>
        /// Adds a plane to the scene.
        /// </summary>
        /// <param name="plane">The plane.</param>
        public void Add(Plane plane)
        {
            this.planes.Add(plane);
        }

        /// <summary>
        /// Tests for intersections between the provided ray and every object in the scene.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="intersection">The intersection for the nearest object.</param>
        /// <returns>Whether the ray intersected an object.</returns>
        public bool Intersect(Ray ray, ref Intersection intersection)
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
                switch (minShapeType)
                {
                    case ShapeType.Sphere:
                        normal = Vector3.Normalize(point - this.spheres[minIndex].Center);
                        intersection = new Intersection(ray, minDistance, normal, this.spheres[minIndex].Material);
                        break;
                    case ShapeType.Plane:
                        normal = this.planes[minIndex].Normal;
                        intersection = new Intersection(ray, minDistance, normal, this.planes[minIndex].Material);
                        break;
                }

                return true;
            }

            return false;
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
            const float bias = 0.0001F; // Bias to prevent self-shadowing (shadow acne).

            // Ray from camera.
            var intersection = new Intersection();
            if (Intersect(camera.RayForPixel(px, py, pw, ph), ref intersection))
            {
                // Shadows.
                var shadowIntersection = new Intersection();
                var shadowRayStart = intersection.Point() + intersection.Normal * bias;

                // Direct lighting.
                var color = intersection.Material.Ambient;
                foreach (var light in this.pointLights)
                {
                    var lightVec = light.Position - shadowRayStart;
                    var lightDistSq = lightVec.LengthSquared();
                    var shadowRayDir = Vector3.Normalize(lightVec);
                    if (Intersect(new Ray(shadowRayStart, shadowRayDir), ref shadowIntersection) == false || shadowIntersection.DistanceSq() > lightDistSq)
                    {
                        var intensity = light.Intensity / (4.0F * (float)Math.PI * lightDistSq); // Inverse square law.
                        intensity *= Math.Max(0.0F, Vector3.Dot(shadowRayDir, intersection.Normal)); // Surface effect.
                        var lightColor = light.Color * intensity;
                        var diffuse = lightColor * intersection.Material.Diffuse;
                        var halfVector = Vector3.Normalize(shadowRayDir - intersection.Ray.Direction);
                        var specular = lightColor * intersection.Material.Specular * (float)Math.Pow(Math.Max(0.0F, Vector3.Dot(intersection.Normal, halfVector)), intersection.Material.Shininess);
                        color += diffuse + specular;
                    }
                }

                foreach (var light in this.directionalLights)
                {
                    var shadowRayDir = -light.Direction;
                    if (Intersect(new Ray(shadowRayStart, shadowRayDir), ref shadowIntersection) == false)
                    {
                        var intensity = light.Intensity * Math.Max(0.0F, Vector3.Dot(shadowRayDir, intersection.Normal)); // Surface effect.
                        var lightColor = light.Color * intensity;
                        var diffuse = lightColor * intersection.Material.Diffuse;
                        var halfVector = Vector3.Normalize(shadowRayDir - intersection.Ray.Direction);
                        var specular = lightColor * intersection.Material.Specular * (float)Math.Pow(Math.Max(0.0F, Vector3.Dot(intersection.Normal, halfVector)), intersection.Material.Shininess);
                        color += diffuse + specular;
                    }
                }

                return Color.Clamp(Color.CorrectGamma(color, camera.Exposure, 1.0F / camera.Gamma));
            }

            return this.background;
        }
    }
}
