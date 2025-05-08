using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MyRhinoPlugin1.models;
using MyRhinoPlugin1.service;

namespace MyRhinoPlugin1.controllers.importController
{
    public class CurvesFromTxtReaderController
    {
    
        public List<Curve> PlatesReader(string filePath)
        {
            int skaleFactor = 1000;
            bool firstLoop = true;

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file {filePath} does not exist.");
            }


            double tempX = 0;
            List<Curve> HullCurves = new List<Curve>();
            /*
            CerveCreator cerveCreator = new CerveCreator("MainHullCurveSection", points);
            hullCurve = cerveCreator.CreateSafeCurve();
            */

            List<Point3d> pointsForOneCurve = new List<Point3d>();

            foreach (string line in File.ReadLines(filePath))
            {

                string[] parts = Regex.Split(line.Trim(), @"\s+");
              
                if (parts.Length < 3) continue;

                   if (firstLoop)
                    {
                        Point3d point = new Point3d(ParseDouble(parts[0]) * skaleFactor, ParseDouble(parts[1]) * skaleFactor, ParseDouble(parts[2]) * skaleFactor);
                        pointsForOneCurve.Add(point);
                        tempX = point.X;
                        firstLoop = false;
                    }
                    else
                    {
                        Point3d point = new Point3d(ParseDouble(parts[0]) * skaleFactor, ParseDouble(parts[1]) * skaleFactor, ParseDouble(parts[2]) * skaleFactor);
                    if (point.X == tempX)
                        {
                            pointsForOneCurve.Add(point);
                        }
                        else
                        {
                            CerveCreator cerveCreator = new CerveCreator("MainHullCurveSection", pointsForOneCurve);
                            HullCurves.Add(cerveCreator.CreateSafeCurve());
                            pointsForOneCurve = new List<Point3d>();
                            Point3d newPoint = new Point3d(ParseDouble(parts[0]) * skaleFactor, ParseDouble(parts[1]) * skaleFactor, ParseDouble(parts[2]) * skaleFactor);
                        pointsForOneCurve.Add(newPoint);
                            tempX = newPoint.X; 
                        }
                    
                }
            } 


            return HullCurves;
        }

        private List<Point3d> mirrorPoints(List<Point3d> pointsForOneCurve)
        {
            foreach (Point3d point in pointsForOneCurve)
            {
               if (point.Y != 0)
                {
                    Point3d mirroredPoint = new Point3d(point.X, -point.Y, point.Z);
                    pointsForOneCurve.Add(mirroredPoint);
                }
            }
            return pointsForOneCurve;

        }

        public static double ParseDouble(string input)
        {
            return double.Parse(input, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
