using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Rhino.Geometry;
using System.Globalization;
using Rhino;

namespace MyRhinoPlugin1.controllers
{
    public class PointsFromTxtReader
    {
        public static List<Point3d> PointsReader(string filePath)
        {
            List<Point3d> points = new List<Point3d>();

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file {filePath} does not exist.");
            }

            foreach (string line in File.ReadLines(filePath))
            {


                // Remove index and curly braces like "1. {" and "}"
                int braceStart = line.IndexOf('{');
                int braceEnd = line.IndexOf('}');
                if (braceStart >= 0 && braceEnd > braceStart)
                {
                    string clean = line.Substring(braceStart + 1, braceEnd - braceStart - 1); // Get the content inside the braces
                    var parts = clean.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length >= 3 &&
                        double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double x) &&
                        double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double y) &&
                        double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double z))
                    {
                        //RhinoApp.WriteLine($"Parsed point: {x}, {y}, {z}");
                        points.Add(new Point3d(x, y, z));

                    }
                }
            }

            return points;
        }
    }
}
