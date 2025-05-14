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

            topClippingPlaneMaker(z);
            fwdClippingPlaneMaker(x);
            isometricClippingPlaneMaker(x, z); // Pass both X and Z to isometricClippingPlaneMaker
        }
        private void isometricClippingPlaneMaker(double x, double z)
        {
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null)
                return;

            // Find isometric view
            RhinoView isoView = null;
            foreach (var v in doc.Views.GetViewList(true, false))
            {
                if (v.ActiveViewport.Name.ToLower().Contains(CustomViewportLayoutCommand.IsometricViewName.ToLower()))
                {
                    isoView = v;
                    break;
                }
            }

            if (isoView == null)
            {
                RhinoApp.WriteLine($"{CustomViewportLayoutCommand.IsometricViewName} view not found.");
                return;
            }

            doc.Views.ActiveView = isoView;

            // Define the plane at Z height
            Plane plane = new Plane(new Point3d(0, 0, z), new Vector3d(0, 0, -100));
            double uLength = 10000;
            double vLength = 10000;

            // Delete existing clipping planes
            var existing = doc.Objects.FindClippingPlanesForViewport(isoView.ActiveViewport);
            foreach (var cp in existing)
                doc.Objects.Delete(cp, true);

            // Add new clipping plane
            Guid cpId = doc.Objects.AddClippingPlane(plane, uLength, vLength, new List<Guid> { isoView.ActiveViewportID });

            if (cpId != Guid.Empty)
            {
                isoView.ActiveViewport.ZoomExtents();
                isoView.Redraw();
                doc.Views.Redraw();
                RhinoApp.WriteLine($"Clipping plane created at X = {x:0.00}, Z = {z:0.00}");
            }

        }
        private void fwdClippingPlaneMaker(double x)
        {
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null)
                return;
            // Find Aft view

            RhinoView fwdView = null;
            foreach (var v in doc.Views.GetViewList(true, false))
            {
                if (v.ActiveViewport.Name.ToLower().Contains(CustomViewportLayoutCommand.FwdViewName.ToLower()))
                {
                    fwdView = v;
                    break;
                }
            }

            if (fwdView == null)
            {
                RhinoApp.WriteLine($"{CustomViewportLayoutCommand.FwdViewName} view not found.");
                return;
            }

            doc.Views.ActiveView = fwdView;
            // Define the plane at X length
            Plane plane = new Plane(new Point3d(x, 0, 0), new Vector3d(-100, 0, 0));
            double uLength = 10000;
            double vLength = 10000;

            // Delete existing clipping planes
            var existing = doc.Objects.FindClippingPlanesForViewport(fwdView.ActiveViewport);
            foreach (var cp in existing)
                doc.Objects.Delete(cp, true);

            // Add new clipping plane
            Guid cpId = doc.Objects.AddClippingPlane(plane, uLength, vLength, new List<Guid> { fwdView.ActiveViewportID });



            if (cpId != Guid.Empty)
            {

                fwdView.ActiveViewport.ZoomExtents();
                fwdView.Redraw();
                doc.Views.Redraw();
                RhinoApp.WriteLine($"Clipping plane created at X = {x:0.00}");
            }
        }

        private void topClippingPlaneMaker(double z)
        {
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null)
                return;

            // Find top view
            RhinoView topView = null;
            foreach (var v in doc.Views.GetViewList(true, false))
            {
                if (v.ActiveViewport.Name.ToLower().Contains(CustomViewportLayoutCommand.TopViewName.ToLower()))
                {
                    topView = v;
                    break;
                }
            }

            if (topView == null)
            {
                RhinoApp.WriteLine($"{CustomViewportLayoutCommand.TopViewName} view not found.");
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
