using Rhino.Geometry.Intersect;
using Rhino.Geometry;
using System.Collections.Generic;
using Rhino;


namespace MyRhinoPlugin1.service
{
    public class GravityFunction
    {

        public static bool TrySnapToGround(Brep movingBox, IEnumerable<Brep> groundBreps, out Transform moveDownTransform)
        {
            moveDownTransform = Transform.Identity;

            // Find the bottom center point of the bounding box
            // Get bounding box and sample multiple points on bottom face
            var bbox = movingBox.GetBoundingBox(true);
            var bottomCenter = new Point3d((bbox.Min.X + bbox.Max.X) / 2, (bbox.Min.Y + bbox.Max.Y) / 2, bbox.Min.Z);

            // Raycast down
            var ray = new Ray3d(bottomCenter, -Vector3d.ZAxis);
            Point3d? closestPoint = null; // The ? makes it a nullable Point3d.
            double closestDist = double.MaxValue;
            // checking which ground object is directly below a box and how far away it is.
            foreach (var ground in groundBreps) 
            {
                // sends a ray into Rhino geometry to see where it intersects
                // to make them into the array.
                // 1 means you only want the first intersection point.
                var hits = Intersection.RayShoot(ray, new[] { ground }, 1);
                //Ensures that a valid hit was found(non - null and at least one point in the array).
                if (hits != null && hits.Length > 0)
                {
                    var pt = hits[0]; // // Take the first intersection point
                    var dist = bottomCenter.DistanceTo(pt); // // Measure how far down it is
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestPoint = pt;
                    }
                }
            }

            if (closestPoint.HasValue)
            {
                RhinoApp.WriteLine($"Closest point found at {closestPoint.Value}");
                double dz = closestPoint.Value.Z - bottomCenter.Z;
                moveDownTransform = Transform.Translation(0, 0, dz);
                return true;
            }

            return false;
        }
    } 
}
