using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;
using Rhino;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.UI;

namespace MyRhinoPlugin1.commands
{
    public class CrossSectionMouseListener : MouseCallback
    {
        protected override void OnMouseDown(MouseCallbackEventArgs e)
        {
            // Only run on left-click
            if (e.MouseButton != MouseButton.Left)
                return;

            var view = e.View;
            if (view == null || !view.ActiveViewport.Name.ToLower().Contains("front"))
                return;

            // Convert mouse position to 3D point
            Line line;
            if (!e.View.ActiveViewport.GetFrustumLine(e.ViewportPoint.X, e.ViewportPoint.Y, out line))
                return;

            // Get point in world space (choose Z coordinate from near point)
            double z = line.From.Z;

            RhinoApp.WriteLine($"Clicked Z height: {z:0.00}");

            CreateClippingPlaneAtZ(z);
            //SetCameraViewAtZ(z);
        }

        private void CreateClippingPlaneAtZ(double z)
        {
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null)
                return;

            // Find top view
            RhinoView topView = null;
            foreach (var v in doc.Views.GetViewList(true, false))
            {
                if (v.ActiveViewport.Name.ToLower().Contains("top"))
                {
                    topView = v;
                    break;
                }
            }

            if (topView == null)
            {
                RhinoApp.WriteLine("Top view not found.");
                return;
            }

            doc.Views.ActiveView = topView;

            // Define the plane at Z height
            Plane plane = new Plane(new Point3d(0, 0, z), new Vector3d(0, 0, -100));
            double uLength = 10000;
            double vLength = 10000;

            // Delete existing clipping planes
            var existing = doc.Objects.FindClippingPlanesForViewport(topView.ActiveViewport);
            foreach (var cp in existing)
                doc.Objects.Delete(cp, true);

            // Add new clipping plane
            Guid cpId = doc.Objects.AddClippingPlane(plane, uLength, vLength, new List<Guid> { topView.ActiveViewportID });

            if (cpId != Guid.Empty)
            {
                topView.ActiveViewport.ZoomExtents();
                topView.Redraw();
                doc.Views.Redraw();
                RhinoApp.WriteLine($"Clipping plane created at Z = {z:0.00}");
            }
        }

      

    }
}
