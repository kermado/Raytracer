namespace Raytracer
{
    public struct Intersection
    {
        public readonly Ray Ray;
        public readonly float Distance;

        public Intersection(Ray ray, float distance)
        {
            Ray = ray;
            Distance = distance;
        }
    }
}
