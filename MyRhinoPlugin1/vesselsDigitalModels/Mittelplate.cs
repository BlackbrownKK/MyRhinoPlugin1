using System;
using System.Collections.Generic;
using Rhino.Geometry;
using MyRhinoPlugin1.models;

namespace MyRhinoPlugin1.vesselsDigitalModels
{
    public class Mittelplate
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

        public List<TDModel> TDList { get; set; }

        public double TDAltitudeLowerPosition { get; set; }
        public double TDAltitudeUpperPosition { get; set; }

        public  double standartLength { get; set; }
        public  double standartWidth { get; set; }
        public  double standartHeight { get; set; }
        public  double TD1Length { get; set; }
        public double TD1Width { get; set; }
        public double TDA_TD1 { get; set; }
        public double TDB_TD1 { get; set; }
        public double TDA_TD2 { get; set; }
        public double TDB_TD2 { get; set; }
        public double firstOffset { get; set; }
        public  double offset { get; set; }

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
            // TDs
            standartLength = 4200;
            standartWidth = 10100;
            standartHeight = 400;
            TD1Length = 4380;
            TD1Width = 7877;
            TDA_TD1 = 0;
            TDB_TD1 = 1654;
            TDA_TD2 = 1271;
            TDB_TD2 = 1106;
            TDAltitudeLowerPosition = 3980;
            TDAltitudeUpperPosition = 4070;
            firstOffset = 36;
            offset = 15;

            // Create a list of Brep objects for the TDs

            TDList = tdCreator();

            VesselElements = new List<Brep>
            {
                FuelTankPS,
                FuelTankSB,
                FWRObliquePS,
                FWRObliqueSB
            };
        
        }


        private List<TDModel> tdCreator()
        {
            Point3d InitialTSPosotion = new Point3d(
            CargoHoldBasePont.X + firstOffset,
            -CargoHoldWidth / 2,
            CargoHoldBasePont.Z + TDAltitudeLowerPosition
        );
            List<TDModel> TDCollection = new List<TDModel>();

            TDCollection.Add(new TDModel("TD_1", TD1Length, TD1Width, standartHeight, TDA_TD1, TDB_TD1, TDAltitudeLowerPosition, TDAltitudeUpperPosition)); // i = 0
            TDCollection.Add(new TDModel("TD_2", standartLength, standartWidth, standartHeight, TDA_TD2, TDB_TD2, TDAltitudeLowerPosition, TDAltitudeUpperPosition)); // i = 1
            TDCollection.Add(new TDModel("TD_3", standartLength, standartWidth, standartHeight, 0, 0, TDAltitudeLowerPosition, TDAltitudeUpperPosition)); // i = 2
            TDCollection.Add(new TDModel("TD_4", standartLength, standartWidth, standartHeight, 0, 0, TDAltitudeLowerPosition, TDAltitudeUpperPosition)); // i = 3
            TDCollection.Add(new TDModel("TD_5", standartLength, standartWidth, standartHeight, 0, 0, TDAltitudeLowerPosition, TDAltitudeUpperPosition)); // i = 4
            TDCollection.Add(new TDModel("TD_6", standartLength, standartWidth, standartHeight, 0, 0, TDAltitudeLowerPosition, TDAltitudeUpperPosition)); // i = 5
            TDCollection.Add(new TDModel("TD_7", standartLength, standartWidth, standartHeight, 0, 0, TDAltitudeLowerPosition, TDAltitudeUpperPosition)); // i = 6
            TDCollection.Add(new TDModel("TD_8", standartLength, standartWidth, standartHeight, 0, 0, TDAltitudeLowerPosition, TDAltitudeUpperPosition)); // i = 7
            TDCollection.Add(new TDModel("TD_9", standartLength, standartWidth, standartHeight, 0, 0, TDAltitudeLowerPosition, TDAltitudeUpperPosition)); // i = 8
            TDCollection.Add(new TDModel("TD_10", standartLength, standartWidth, standartHeight, 0, 0, TDAltitudeLowerPosition, TDAltitudeUpperPosition)); // i = 9
            TDCollection.Add(new TDModel("TD_11", standartLength, standartWidth, standartHeight, 0, 0, TDAltitudeLowerPosition, TDAltitudeUpperPosition)); // i = 10
            TDCollection.Add(new TDModel("TD_12", standartLength, standartWidth, standartHeight, 0, 0, TDAltitudeLowerPosition, TDAltitudeUpperPosition)); // i = 11
            TDCollection.Add(new TDModel("TD_13", standartLength, standartWidth, standartHeight, 0, 0, TDAltitudeLowerPosition, TDAltitudeUpperPosition)); // i = 12
            TDCollection.Add(new TDModel("TD_14", standartLength, standartWidth, standartHeight, 0, 0, TDAltitudeLowerPosition, TDAltitudeUpperPosition)); // i = 13

 

            // set the position points
            TDCollection[13].LocationOfPosition = new Point3d(InitialTSPosotion.X, InitialTSPosotion.Y, InitialTSPosotion.Z);
            for (int i = 12; i >= 0; i--) // 12 ... 0
            {
                if (TDCollection[i].Width == TDCollection[i + 1].Width)
                {
                    TDCollection[i].LocationOfPosition =
                  new Point3d(TDCollection[i + 1].LocationOfPosition.X + standartLength + offset, InitialTSPosotion.Y, InitialTSPosotion.Z);
                }
                else
                {
                    double delta = TDCollection[i + 1].Width - TDCollection[i].Width;
                    TDCollection[i].LocationOfPosition =
                 new Point3d(TDCollection[i + 1].LocationOfPosition.X + standartLength + offset, InitialTSPosotion.Y + delta/2, InitialTSPosotion.Z);
                }  
            }

            foreach (TDModel model in TDCollection)
            {
                model.TDAltitudeLowerPosition = TDAltitudeLowerPosition;
                model.TDAltitudeUpperPosition = TDAltitudeUpperPosition;
            }

            return TDCollection;
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

            else
            {

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



        private PolylineCurve drawFWROblique(double l, double w, double h, String SBPS)
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
