using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.UI;
using System;
using System.Collections.Generic;



namespace MyRhinoPlugin1.models
{
    public class TDObjectCreator
    {

        public List<TDModel> TDBrepCreator(List<TDModel> TDList)
        {
            List <TDModel> TDCollecttion = new List<TDModel>();

            foreach (TDModel model in TDList)
            {
                if (model.TDB == 0)
                {
                    // Define min and max points
                    Point3d minPoint = model.LocationOfPosition;
                    Point3d maxPoint = new Point3d(
                        model.LocationOfPosition.X + model.Length,
                        model.LocationOfPosition.Y + model.Width,
                        model.LocationOfPosition.Z + model.Height
                        );
                    // Create BoundingBox
                    BoundingBox bbox = new BoundingBox(minPoint, maxPoint);
                    // Convert BoundingBox to Brep
                    model.TDModelBrep = Brep.CreateFromBox(bbox);
                    TDCollecttion.Add(model);


                }
                else
                {
                    Brep TDNonSt = createNotStandartTD(model);
                    if (TDNonSt != null) model.TDModelBrep = TDNonSt;
                    TDCollecttion.Add(model);
                } 
            }
            return TDCollecttion;
        }

        public Brep createNotStandartTD(TDModel model)
        {
            double length = model.Length;
            double width = model.Width;
            double height = model.Height;
            double a = model.TDA;
            double b = model.TDB;
            Point3d baseOrigin = model.LocationOfPosition;
            // Define 2D points in XY plane at baseOrigin.Z
            Point3d pt0 = new Point3d(baseOrigin.X, baseOrigin.Y, baseOrigin.Z);
            Point3d pt1 = new Point3d(baseOrigin.X, baseOrigin.Y + width, baseOrigin.Z);
            Point3d pt2 = new Point3d(baseOrigin.X + a, baseOrigin.Y + width, baseOrigin.Z);
            Point3d pt3 = new Point3d(baseOrigin.X + length, baseOrigin.Y + width - b, baseOrigin.Z);
            Point3d pt4 = new Point3d(baseOrigin.X + length, baseOrigin.Y + b, baseOrigin.Z);
            Point3d pt5 = new Point3d(baseOrigin.X + a, baseOrigin.Y, baseOrigin.Z);

            List<Point3d> points = new List<Point3d> { pt0, pt1, pt2, pt3, pt4, pt5, pt0 };

            // Remove adjacent duplicates
            for (int i = points.Count - 2; i >= 0; i--)
            {
                if (points[i].DistanceTo(points[i + 1]) < RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                    points.RemoveAt(i);
            }

            PolylineCurve polyline = new PolylineCurve(points);


            // Validate
            if (!polyline.IsClosed || !polyline.IsPlanar())
            {
                RhinoApp.WriteLine("⚠️ Polyline is not closed or not planar.");
                return null;
            }

            Plane plane;
            if (!polyline.TryGetPlane(out plane))
            {
                RhinoApp.WriteLine("⚠️ Could not determine polyline plane.");
                return null;
            }

            // Convert to curve and extrude
            Curve planarCurve = polyline.ToNurbsCurve();
            Vector3d extrusionVector = new Vector3d(0, 0, height);
            Surface surface = Surface.CreateExtrusion(planarCurve, extrusionVector);

            if (surface == null)
            {
                RhinoApp.WriteLine("⚠️ Extrusion surface creation failed.");
                return null;
            }

            Brep brep = surface.ToBrep();
            if (brep == null || !brep.IsValid)
            {
                RhinoApp.WriteLine("⚠️ Failed to convert surface to valid Brep.");
                return null;
            }

            // Cap the extrusion to make it a solid body (if necessary)
            Brep solidBrep = brep.CapPlanarHoles(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            if (solidBrep != null && solidBrep.IsSolid)
            {
                return solidBrep;
            }

            RhinoApp.WriteLine("⚠️ Failed to cap the extrusion into a solid.");
            return null;
        }
      
    }
}
