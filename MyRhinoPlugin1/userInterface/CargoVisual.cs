using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRhinoPlugin1.userInterface
{
    public class CargoVisual
    {
        public string Name { get; set; }
        public RectangleF Bounds { get; set; } // X, Y, Width, Height
        public Color FillColor { get; set; } = Colors.LightSkyBlue;
    }
}
