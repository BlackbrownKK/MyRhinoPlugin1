using MyRhinoPlugin1.service;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace MyRhinoPlugin1.models
{
    public class PointsCollection
    {
        public Curve CrosSectionPoints { get; set; }
        public Point3d A { get; set; }
        public Point3d B { get; set; }
        public Point3d C { get; set; }
        public Point3d D { get; set; }


        public string Name { get; set; }



        public PointsCollection(String name, Point3d a, Point3d b, Point3d c, Point3d d)
        {
            this.Name = name;
            // Create a curve from the four points
            List<Point3d> points = new List<Point3d> { a, b, c, d };
            CerveCreator curveCreator = new CerveCreator(name, points);
            this.CrosSectionPoints = curveCreator.CreateSafeCurve();
            this.A = a;
            this.B = b;
            this.C = c;
            this.D = d; 
        }
    }
}
