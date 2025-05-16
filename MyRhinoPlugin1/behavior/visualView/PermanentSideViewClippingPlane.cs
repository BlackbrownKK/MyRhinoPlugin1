using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;
using MyRhinoPlugin1.commands;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.Render.ChangeQueue;
using Rhino.UI;

namespace MyRhinoPlugin1.behavior.visualView
{
    public class PermanentSideViewClippingPlane
    {

        

        public void SideClippingPlaneMaker(double YDist)
        {
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null)
                return;

            // Find side view
            RhinoView sideView = null;
            foreach (var v in doc.Views.GetViewList(true, false))
            {
                if (v.ActiveViewport.Name.Equals(CustomViewportLayoutCommand.SideViewName, StringComparison.OrdinalIgnoreCase))
                {
                    sideView = v;
                    break;
                }
            }

            if (sideView == null)
            {
                RhinoApp.WriteLine($"{CustomViewportLayoutCommand.SideViewName} view not found.");
                return;
            }

            doc.Views.ActiveView = sideView;

            // Define a clipping plane centered at the origin (0, 0, 0) with normal in +X direction (YZ plane)
            Plane plane = new Plane(new Point3d(0, -YDist*0.99, 0), Vector3d.YAxis);

            double uLength = 20000; // Width along Y
            double vLength = 20000; // Height along Z

            // Delete existing clipping planes in this viewport
            var existing = doc.Objects.FindClippingPlanesForViewport(sideView.ActiveViewport);
            foreach (var cp in existing)
                doc.Objects.Delete(cp, true);

            // Add the new clipping plane to the viewport
            Guid cpId = doc.Objects.AddClippingPlane(plane, uLength, vLength, new List<Guid> { sideView.ActiveViewportID });

            if (cpId != Guid.Empty)
            {
                RhinoApp.WriteLine("Clipping plane created at X = 0, facing +X (YZ plane).");
                sideView.ActiveViewport.ZoomExtents();
                sideView.Redraw();
                doc.Views.Redraw();
            }
            else
            {
                RhinoApp.WriteLine("Failed to create clipping plane.");
            }
        }

    }
}
