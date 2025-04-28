using Rhino;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace MyRhinoPlugin1.service
{
    public class CerveCreator
    {
        public string Name { get; set; }
        private List<Point3d> Points { get; set; }

        // Constructor
        public CerveCreator(string name, List<Point3d> points)
        {
            Name = name;
            Points = points;
        }

        // Create a curve from the points with full safety
        public Curve CreateSafeCurve()
        {
            if (Points == null || Points.Count < 2)
            {
                RhinoApp.WriteLine($"[{Name}] Error: Point list is empty or too small.");
                return null;
            }

            // Ensure the polyline is closed by appending the start point to the end if needed
            if (!Points.First().Equals(Points.Last()))
            {
                Points.Add(Points.First());
            }

            // Create a polyline from the points
            Polyline polyline = new Polyline(Points);

            // Convert polyline to NurbsCurve
            Curve curve = polyline.ToNurbsCurve();
            if (curve == null)
            {
                RhinoApp.WriteLine($"[{Name}] Error: Failed to create NurbsCurve from points.");
                return null;
            }

            // Check if curve is closed
            if (!curve.IsClosed)
            {
                RhinoApp.WriteLine($"[{Name}] Warning: Curve not closed. Attempting to close...");
                curve.MakeClosed(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            }

            // Check if curve is planar
            if (!curve.IsPlanar())
            {
                RhinoApp.WriteLine($"[{Name}] Warning: Curve not planar. Projecting to WorldXY...");
                curve = Curve.ProjectToPlane(curve, Plane.WorldXY);
            }

            // Simplify the curve
            Curve simplified = curve.Simplify(CurveSimplifyOptions.All,
                                               RhinoDoc.ActiveDoc.ModelAbsoluteTolerance,
                                               RhinoDoc.ActiveDoc.ModelAngleToleranceRadians);
            if (simplified != null && simplified.IsValid)
            {
                curve = simplified;
            }

            // Final validity check
            if (!curve.IsValid)
            {
                RhinoApp.WriteLine($"[{Name}] Error: Curve is still invalid after processing.");
                return null;
            }

            return curve;
        }
    }
}
