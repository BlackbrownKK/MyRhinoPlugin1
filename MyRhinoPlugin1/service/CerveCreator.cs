using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRhinoPlugin1.service
{
    public class CerveCreator
    {
        public string name { get; set; }
        List<Point3d> points = new List<Point3d>();


        // create a curve from a list of points
        public CerveCreator(string name, List<Point3d> points)
        {
            this.name = name;
            this.points = points;
        }

        public Curve CerveCreatorByListOfPoints()
        {
            // Ensure the polyline is closed by appending the start point to the end if needed
            if (points.Count > 1 && points.First() != points.Last())
            {
                points.Add(points.First());
            }
            // Create a polyline from the points
            Polyline polyline = new Polyline(points);
            // Create a curve from the polyline
            Curve curve = polyline.ToNurbsCurve();
            // Check if the curve is valid
            if (curve == null || !curve.IsValid)
            {
                Rhino.RhinoApp.WriteLine("Error: Curve creation failed.");
                return null;
            }
            return curve;
        }
            

    }
}
