using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.UI;
namespace MyRhinoPlugin1.models
{
    public class TDModel
    {

        public string Name { get; set; }
        public Brep TDModelBrep { get; set; }
        public RhinoObject TDModelObject { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double TDAltitudeLowerPosition { get; set; } 
        public double TDAltitudeUpperPosition { get; set; }

        public double TDA { get; set; }
        public double TDB { get; set; }

        public Point3d LocationOfPosition { get; set; }

        public Layer vesselTDLayer { get; set; }



        public TDModel(string name, double length, double width, double height,  double tDA, double tDB, double tDAltitudeLowerPosition, double tDAltitudeUpperPosition)
        {
            Name = name;
            this.Length = length;
            this.Width = width;
            this.Height = height;
            TDAltitudeLowerPosition = tDAltitudeLowerPosition;
            TDAltitudeUpperPosition = tDAltitudeUpperPosition;
            TDA = tDA;
            TDB = tDB;
        }
    }
}
