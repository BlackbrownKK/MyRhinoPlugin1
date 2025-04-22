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


        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            customViewsMaker(doc, mode);
            return Result.Success;
        }


        public static void customViewsMaker(RhinoDoc doc, RunMode mode)
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
            viewports[0].ActiveViewport.SetProjection(DefinedViewportProjection.Perspective, "Isometric View", false);
            viewports[1].ActiveViewport.SetProjection(DefinedViewportProjection.Front, "Front View", false);
            viewports[2].ActiveViewport.SetProjection(DefinedViewportProjection.Top, "Top View", false);
            viewports[3].ActiveViewport.SetProjection(DefinedViewportProjection.Right, "Right View", false);


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
    }
}