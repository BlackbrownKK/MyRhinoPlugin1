using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;
using MyRhinoPlugin1.commands;
using MyRhinoPlugin1.behavior.visualView;

using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.Render.ChangeQueue;
using Rhino.UI;

namespace MyRhinoPlugin1.behavior.visualView
{
    public class CrossSectionMouseListener : MouseCallback
    {
        protected override void OnMouseDown(MouseCallbackEventArgs e)
        {
            // Only run on left-click
            if (e.MouseButton != MouseButton.Left)
                return;

            var view = e.View;
            if (view == null || !view.ActiveViewport.Name.ToLower().Contains(CustomViewportLayoutCommand.SideViewName.ToLower()))
                return;

            // Convert mouse position to 3D point
            Line line;
            if (!e.View.ActiveViewport.GetFrustumLine(e.ViewportPoint.X, e.ViewportPoint.Y, out line))
                return;

            // Get point in world space (choose Z coordinate from near point)
            double z = line.From.Z;
            double x = line.From.X;

            RhinoApp.WriteLine($"Clicked Z height: {z:0.00}; X height: {x:0.00}");

            ClippingPlaneMaker.topClippingPlaneMaker(z);
            ClippingPlaneMaker.fwdClippingPlaneMaker(x);
            ClippingPlaneMaker.isometricClippingPlaneMaker(x, z); // Pass both X and Z to isometricClippingPlaneMaker
        }
    }
}
