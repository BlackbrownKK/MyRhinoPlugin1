using Rhino.Commands;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Display;
using Rhino.DocObjects;

namespace MyRhinoPlugin1.commands
{
    public class CustomViewportLayoutCommand : Command
    {
        public override string EnglishName => "CustomViews";

        public static string IsometricViewName = "Isometric view";
        public static string SideViewName = "Side view";
        public static string TopViewName = "Top view";
        public static string FwdViewName = "FWD view";
       



        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            custom2ViewsMaker(doc, mode);
            return Result.Success;
        }


        public static void custom4ViewsMaker(RhinoDoc doc, RunMode mode)
        {

  

            // Reset to 4-view layout
            RhinoApp.RunScript("_4View _Enter", false);

            // Get list of views
            var views = doc.Views.GetViewList(true, false);
            var viewports = new List<RhinoView>(views);

            if (viewports.Count < 4)
            {
                RhinoApp.WriteLine("Expected 4 viewports, found " + viewports.Count);
            }

            // Set specific projections
            viewports[0].ActiveViewport.SetProjection(DefinedViewportProjection.Perspective, IsometricViewName, false);
            viewports[1].ActiveViewport.SetProjection(DefinedViewportProjection.Front, SideViewName, false);
            viewports[2].ActiveViewport.SetProjection(DefinedViewportProjection.Top, TopViewName, false);
            viewports[3].ActiveViewport.SetProjection(DefinedViewportProjection.Right, FwdViewName, false);


            // Optionally adjust camera for perspective
            var perspectiveVP = viewports[3].ActiveViewport;
            perspectiveVP.SetCameraLocation(new Rhino.Geometry.Point3d(50, -50, 50), true);
            perspectiveVP.SetCameraTarget(Rhino.Geometry.Point3d.Origin, true);

            // Redraw views
            foreach (var v in viewports)
            {
                v.Redraw();
                v.ActiveViewport.ZoomExtents();
            }

        }

        public static void custom2ViewsMaker(RhinoDoc doc, RunMode mode)
        {

            // Close all existing views
            foreach (var view in doc.Views)
                view.Close();
            var views = doc.Views.GetViewList(true, false);

            // Create first view (Side / Front)
            var sideView = doc.Views.Add(SideViewName, DefinedViewportProjection.Front, new System.Drawing.Rectangle(0, 0, 1500, 400), false);
            sideView.ActiveViewport.Name = SideViewName;
            views[0] = sideView;
            views[0].Redraw();

          

            // Create second view (Top) below it
            var topView = doc.Views.Add(TopViewName, DefinedViewportProjection.Top, new System.Drawing.Rectangle(0, 400, 1500, 400), false);
            topView.ActiveViewport.Name = TopViewName;
            topView.Redraw();

            // Zoom all
            sideView.ActiveViewport.ZoomExtents();
            topView.ActiveViewport.ZoomExtents();
        }
    }
}