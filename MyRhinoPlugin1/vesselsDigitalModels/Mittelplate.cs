using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;
namespace MyRhinoPlugin1.vesselsDigitalModels
{
    internal class Mittelplate
    {

        public Point3d APPoint { get; set; }
        public Point3d FPPoint { get; set; }

        public Point3d CargoHoldBasePont { get; set; }
        public Double CargoHoldLength { get; set; }
        public Double CargoHoldWidth { get; set; }
        public Double CargoHoldHeight { get; set; }

        public Double VesselsLengthOA { get; set; }
        public Double VesselsBreadth { get; set; }

        public double FuelTankLength { get; set; }
        public double FuelTankWidth { get; set; }
        public double FuelTankHeight { get; set; }
        public Brep FuelTankPS { get; set; }
        public Brep FuelTankSB { get; set; }

        public Brep FWRObliquePS { get; set; }
        public Brep FWRObliqueSB { get; set; }

        public List<Brep> TDs { get; set; }


        public List<Brep> VesselElements { get; set; }


        public Mittelplate()
        {
            APPoint = new Point3d(0, 0, 0);
            FPPoint = new Point3d(82420, 0, 0);
            CargoHoldBasePont = new Point3d(13900, 0, 950);
            CargoHoldLength = 59250;
            CargoHoldWidth = 10200;
            CargoHoldHeight = 8472;
            VesselsBreadth = 12400;
            VesselsLengthOA = 86000;
            FuelTankLength = 1500;
            FuelTankWidth = 2500;
            FuelTankHeight = 2550;
            FuelTankPS = drawFuelTank(true);
            FuelTankSB = drawFuelTank(false);
            // Create an extrusion
            Extrusion extrusion = Extrusion.Create(drawFWROblique(7425, 2800, CargoHoldHeight, "PS"), -CargoHoldHeight, true);
            FWRObliquePS = extrusion.ToBrep(true);
            extrusion = Extrusion.Create(drawFWROblique(7425, 2800, CargoHoldHeight, "SB"), CargoHoldHeight, true);
            FWRObliqueSB = extrusion.ToBrep(true);

            // Create a list of Brep objects for the TDs

            TDs = tdCreator();

            VesselElements = new List<Brep>
            {
                FuelTankPS,
                FuelTankSB,
                FWRObliquePS,
                FWRObliqueSB
            };

        }


        private List<Brep> tdCreator()
        {

            List<Brep> TDCollection = new List<Brep>();

            double length = 4200;
            double width = 10100;
            double height = 400;

            double firstOffset = 36;
            double offset = 15;
            double TDAltitudeLowerPosition = 3980;

            int TDStandartCount = 13; // Number of TDs to create from 3 to 14


            Point3d baseOrigin = new Point3d(
                CargoHoldBasePont.X + firstOffset,
                -CargoHoldWidth / 2, 
                CargoHoldBasePont.Z + TDAltitudeLowerPosition
            );

            double tempXposition = 0;
            for (int i = 0; i <= TDStandartCount-2; i++) // TDs 3-14
            {
                    // Adjusted base point for TDs
                    baseOrigin = new Point3d(
                        CargoHoldBasePont.X + (length + offset) * i,
                        -CargoHoldWidth / 2, 
                        CargoHoldBasePont.Z + TDAltitudeLowerPosition
                    );
                // Create the TD and add it to the collection
                TDCollection.Add(drawStandartTD(baseOrigin, length, width, height));
                tempXposition = baseOrigin.X;
            }
            // create the TD#2
            baseOrigin = new Point3d(
                 tempXposition + length + offset,
                 -CargoHoldWidth / 2, 
                 CargoHoldBasePont.Z + TDAltitudeLowerPosition
             );
            double TDA = 2929;
            double TDB = 1106;
            TDCollection.Add(drawNotStandartTD(baseOrigin, length, width, height, TDA, TDB));
            tempXposition = baseOrigin.X;

            // create the TD#1
            double TD1Length = 4380;
            double TD1Width = 7877;
             TDA = 4380;
             TDB = 1654;
            double TD1WidthDelta = width - TD1Width;

            baseOrigin = new Point3d(
              tempXposition + length + offset,
              -CargoHoldWidth / 2 + TD1WidthDelta/2, 
              CargoHoldBasePont.Z + TDAltitudeLowerPosition
          );
            TDCollection.Add(drawNotStandartTD(baseOrigin, TD1Length, TD1Width, height, TDA, TDB));

            return TDCollection;

        }

                 // TDs 2


            
        
        private Brep drawStandartTD(Point3d baseOrigin, double length, double width, double height)
        {
            // Define min and max points
            Point3d minPoint = baseOrigin;
            Point3d maxPoint = new Point3d(
                baseOrigin.X + length,
                baseOrigin.Y + width,
                baseOrigin.Z + height
            );
            // Create BoundingBox
            BoundingBox bbox = new BoundingBox(minPoint, maxPoint);
            // Convert BoundingBox to Brep
            return Brep.CreateFromBox(bbox);
        }

        private Brep drawNotStandartTD(Point3d baseOrigin, double length, double width, double height, double a, double b)
        {
            // Step 1: Create the main box

            // Define min and max points
            Point3d minPoint = baseOrigin;
            Point3d maxPoint = new Point3d(
                baseOrigin.X + length,
                baseOrigin.Y + width,
                baseOrigin.Z + height
            );
            // Create BoundingBox
            BoundingBox bbox = new BoundingBox(minPoint, maxPoint);
            Brep boxBrep = Brep.CreateFromBox(bbox);
            if (boxBrep == null)
                return null; // Fail-safe check

            // Step 2: Define the triangular prism (cut-out shape)
            Point3d point1 = maxPoint;  // Top-right corner of the box
            Point3d point2 = new Point3d(maxPoint.X - a, maxPoint.Y, maxPoint.Z);
            Point3d point3 = new Point3d(maxPoint.X, maxPoint.Y - b, maxPoint.Z);
            Point3d ExtraPoint = new Point3d(maxPoint.X+100, maxPoint.Y, maxPoint.Z);
            Point3d mirroredPoint1 = new Point3d(maxPoint.X, maxPoint.Y - width, maxPoint.Z);  // Opposite top-left corner
            Point3d mirroredPoint2 = new Point3d(maxPoint.X - a, maxPoint.Y - width, maxPoint.Z);
            Point3d mirroredPoint3 = new Point3d(maxPoint.X, maxPoint.Y - width + b, maxPoint.Z);
            Point3d mirroredExtraPoint = new Point3d(maxPoint.X + 100, maxPoint.Y - width, maxPoint.Z);

            PolylineCurve triangle = new PolylineCurve(new[] { point1, ExtraPoint, mirroredExtraPoint, mirroredPoint1, mirroredPoint2, mirroredPoint3, point3, point2, point1 });

            // Step 3: Extrude the triangle downwards by -height
            Vector3d extrusionVector = new Vector3d(0, 0, -height);
            Brep prismBrep1 = Extrusion.Create(triangle, -height, true)?.ToBrep();

            if (prismBrep1 == null)
                return boxBrep; // Fail-safe: return just the box if prism creation fails

 


           
 
           
            // Step 5: Perform Boolean Difference (subtract both prisms)
            Brep[] firstCut = Brep.CreateBooleanDifference(boxBrep, prismBrep1, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            if (firstCut == null || firstCut.Length == 0)
            {
                RhinoApp.WriteLine("Error: First Boolean Difference failed.");
                return boxBrep;
            }

 


            return firstCut[0]; // Return the modified box
        }



        private Brep drawFuelTank(Boolean PSSB)
        {
            Point3d baseOrigin;
            Point3d minPoint;
            Point3d maxPoint;

            if (PSSB)
            {
                // Adjusted base point for FuelTankPS
                baseOrigin = new Point3d(
                   CargoHoldBasePont.X,
                   (CargoHoldWidth / 2) - 2500, // Adjusted Y position
                   CargoHoldBasePont.Z
               );
            }

            else {

                // Adjusted base point for FuelTankPS
                baseOrigin = new Point3d(
                   CargoHoldBasePont.X,
                   (-CargoHoldWidth / 2), // Adjusted Y position
                   CargoHoldBasePont.Z
               );
            }

            // Define min and max points
            minPoint = baseOrigin;
            maxPoint = new Point3d(
               baseOrigin.X + FuelTankLength,
               baseOrigin.Y + FuelTankWidth,
               baseOrigin.Z + FuelTankHeight
           );

            // Create BoundingBox
            BoundingBox bbox = new BoundingBox(minPoint, maxPoint);

            // Convert BoundingBox to Brep
            return Brep.CreateFromBox(bbox);
        }



        private PolylineCurve drawFWROblique(double l, double w, double h, String SBPS )
        {
            Point3d pointA = new Point3d();
            Point3d pointB = new Point3d();
            Point3d pointC = new Point3d();

            if (SBPS.Equals("PS"))
            {
                // Adjusted base points 
                  pointA = new Point3d(
                    CargoHoldBasePont.X + CargoHoldLength - l,
                    (CargoHoldWidth / 2), 
                   CargoHoldBasePont.Z
                );

                 pointB = new Point3d(
                    CargoHoldBasePont.X + CargoHoldLength,
                    (CargoHoldWidth / 2), 
                   CargoHoldBasePont.Z
                );
                 pointC = new Point3d(
                    CargoHoldBasePont.X + CargoHoldLength,
                    (CargoHoldWidth / 2) - w,
                   CargoHoldBasePont.Z
                );
            }
            else if (SBPS.Equals("SB"))
            {
                // Adjusted base points 
                 pointA = new Point3d(
                    CargoHoldBasePont.X + CargoHoldLength - l,
                    -(CargoHoldWidth / 2),
                   CargoHoldBasePont.Z
                );
                 pointB = new Point3d(
                    CargoHoldBasePont.X + CargoHoldLength,
                    -(CargoHoldWidth / 2), 
                   CargoHoldBasePont.Z
                );
                 pointC = new Point3d(
                    CargoHoldBasePont.X + CargoHoldLength,
                    -(CargoHoldWidth / 2) + w,
                   CargoHoldBasePont.Z
                );
            }


            // Create the base triangle as a PolylineCurve
            PolylineCurve baseTriangle = new PolylineCurve(new[] { pointA, pointB, pointC, pointA });
            return baseTriangle;
           
        }
    }
}
