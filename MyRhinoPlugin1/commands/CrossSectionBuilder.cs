using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.Display;
using System;

public class CrossSectionBuilder : Command
{
    public override string EnglishName => "PickCrossSection";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        // Ensure the active view is the Front view
        var view = doc.Views.ActiveView;
        if (view == null || !view.ActiveViewport.Name.ToLower().Contains("front view"))
        {
            RhinoApp.WriteLine("Please activate the Front view before running this command.");
            return Result.Failure;
        }
        // Get point from the user to define Z height for the cross-section
        var gp = new GetPoint();
        gp.SetCommandPrompt("Click in the Front view to define Z height for top cross-section");
        gp.Get();

        if (gp.CommandResult() != Result.Success)
            return gp.CommandResult();

        Point3d clickedPoint = gp.Point();
        double z = clickedPoint.Z;
        RhinoApp.WriteLine($"Z height selected: {z:0.00}");

        // Switch to Top view
        RhinoView topView = null;
        foreach (var v in doc.Views.GetViewList(true, false))
        {
            if (v.ActiveViewport.Name.ToLower().Contains("top view"))
            {
                topView = v;
                break;
            }
        }

        if (topView == null)
        {
            RhinoApp.WriteLine("Top view not found.");
            return Result.Failure;
        }

        // Set the active view to Top view
        doc.Views.ActiveView = topView;
        doc.Views.Redraw();

        // Step 4: Create clipping plane at selected Z height
        double size = 10000; // big enough to cut through any geometry
        Point3d origin = new Point3d(0, 0, z);


        // Create a clipping plane at Z = your selected height
        Plane plane2 = new Plane(new Point3d(0, 0, z), new Point3d(0, 1000, z), new Point3d(1000, 1000, z));
        Plane plane = new Plane(new Point3d(0, 0, z), Vector3d.ZAxis);
        double uLength = 10000;
        double vLength = 10000;

        // Add to all views (or restrict to specific views if needed)
        var tempView = doc.Views.Find("top view", true);
        if (topView == null)
        {
            RhinoApp.WriteLine("Top view not found.");
            return Result.Failure;
        }
        var views = new RhinoView[] { topView };

        // Add the clipping plane
        Guid clippingPlaneId = doc.Objects.AddClippingPlane(plane2, uLength, vLength, new System.Collections.Generic.List<Guid> { topView.ActiveViewportID });


        
        if (clippingPlaneId == Guid.Empty)
        {
            RhinoApp.WriteLine("Failed to create clipping plane.");
            return Result.Failure;
        }

        // Redraw and zoom extents in the Top view
        topView.ActiveViewport.ZoomExtents();
        topView.Redraw();
        RhinoApp.WriteLine("Clipping plane created at Z = " + z.ToString("0.00"));
        return Result.Success;
    }
}