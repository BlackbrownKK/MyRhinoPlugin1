
using System.Collections.Generic;
using Rhino.Geometry;
namespace MyRhinoPlugin1.models
{
    public class VesselModel
    {

        public Point3d APPoint { get; set; }
        public Point3d FPPoint { get; set; }

        public Point3d CargoHoldBasePont { get; set; }
        public double CargoHoldLength { get; set; }
        public double CargoHoldWidth { get; set; }
        public double CargoHoldHeight { get; set; }

        public double VesselsLengthOA { get; set; }
        public double VesselsBreadth { get; set; }

        public double TDAltitudeLowerPosition { get; set; }
        public double TDAltitudeUpperPosition { get; set; }

        public List<TDModel> TDList { get; set; }
        public Dictionary<string, Point3d> TDInitialPositionPointList { get; set; } = new Dictionary<string, Point3d>();

        public List<Brep> VesselElements { get; set; }

        public VesselModel()
        {
            APPoint = Point3d.Unset;
            FPPoint = Point3d.Unset;
            CargoHoldBasePont = Point3d.Unset;

            CargoHoldLength = 0.0;
            CargoHoldWidth = 0.0;
            CargoHoldHeight = 0.0;

            VesselsLengthOA = 0.0;
            VesselsBreadth = 0.0;

            TDList = new List<TDModel>();
            VesselElements = new List<Brep>();
        }

    }
}
