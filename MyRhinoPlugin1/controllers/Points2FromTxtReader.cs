using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyRhinoPlugin1.models;
using System.IO;
using System.Text.RegularExpressions;
using Rhino.Geometry;
using Rhino;


namespace MyRhinoPlugin1.controllers
{
    public class Points2FromTxtReader
    {

        public static List<PointsCollection> PlatesReader(string filePath)
        {
            List<PointsCollection> plates = new List<PointsCollection>();
            int skaleFactor = 1000;
            // Check if the file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file {filePath} does not exist.");
            }

            foreach (string line in File.ReadLines(filePath))
            {
              
                string[] parts = Regex.Split(line.Trim(), @"\s+");
                Point3d a = new Point3d(ParseDouble(parts[1]), ParseDouble (parts[2]), ParseDouble (parts[3]));
                Point3d b = new Point3d(ParseDouble(parts[4]), ParseDouble(parts[5]), ParseDouble(parts[6]));
                Point3d c = new Point3d(ParseDouble(parts[7]), ParseDouble(parts[8]), ParseDouble(parts[9]));
                Point3d d = new Point3d(ParseDouble(parts[10]), ParseDouble(parts[11]), ParseDouble(parts[12]));
                PointsCollection segment = new PointsCollection(parts[0], a*skaleFactor, b* skaleFactor, c * skaleFactor,d* skaleFactor);
                plates.Add(segment);
            }
        

            return plates;
        }

        public static double ParseDouble(string input)
        {
            return double.Parse(input, System.Globalization.CultureInfo.InvariantCulture);
        } 
    }
}

 
