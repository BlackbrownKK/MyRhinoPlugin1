using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.Display;

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

        // Wait briefly to ensure the view change is processed
        System.Threading.Thread.Sleep(500); // Wait for 500ms

        // Construct section line at Z height
        Point3d pt1 = new Point3d(-1e6, 0, z);  // Very long line to slice all geometry
        Point3d pt2 = new Point3d(1e6, 0, z);

        // Format points as a proper string
        var culture = System.Globalization.CultureInfo.InvariantCulture;
        string pt1Str = string.Format(culture, "{0},{1},{2}", pt1.X, pt1.Y, pt1.Z);
        string pt2Str = string.Format(culture, "{0},{1},{2}", pt2.X, pt2.Y, pt2.Z);

        // Build the section command string
        string sectionCommand = $"_Section {pt1Str} {pt2Str} _Enter";

        // Run the command
        bool result = RhinoApp.RunScript(sectionCommand, false);

        if (!result)
        {
            RhinoApp.WriteLine("Failed to run _Section command.");
            return Result.Failure;
        }

        // Redraw and zoom extents in the Top view
        topView.ActiveViewport.ZoomExtents();
        topView.Redraw();

        return Result.Success;
    }
}