using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System.Collections.Generic;
using System.Linq;

namespace MyRhinoPlugin1.commands
{
    public class GravityMoveCommandG
    {
        /*
      //  public override string EnglishName => "GravityMoveCommand";

       // protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Step 1: Select Brep
            var go = new GetObject();
            go.SetCommandPrompt("Select cargo unit (Brep) to apply gravity");
            go.GeometryFilter = ObjectType.Brep;
            go.SubObjectSelect = false;
            go.Get();
            if (go.CommandResult() != Result.Success)
                return go.CommandResult();

            var objRef = go.Object(0);
            var brep = objRef?.Brep();
            if (brep == null)
                return Result.Failure;

            // Step 2: Ask user to press 'G'
            var gs = new GetString();
            gs.SetCommandPrompt("Press G to apply gravity");
            gs.AcceptNothing(true);
            gs.Get();
            if (gs.CommandResult() != Result.Success)
                return gs.CommandResult();

            if (!gs.StringResult().Equals("G", System.StringComparison.OrdinalIgnoreCase))
            {
                RhinoApp.WriteLine("Gravity not applied. You didn't press 'G'.");
                return Result.Cancel;
            }

            // Step 3: Collect other Breps as ground
            var groundBreps = doc.Objects
                .Where(o => o.Id != objRef.ObjectId && o.Geometry is Brep)
                .Select(o => o.Geometry as Brep)
                .ToList();

            // Step 4: Apply gravity
            if (service.GravityFunction.TrySnapToGround(brep, groundBreps, out Transform moveDown))
            {
                doc.Objects.Transform(objRef, moveDown, true);
                RhinoApp.WriteLine($"Gravity applied to {objRef.ObjectId}");
                doc.Views.Redraw();
            }
            else
            {
                RhinoApp.WriteLine("No ground found below the object.");
            }

            return Result.Success;
        }
    }
}
        */
    }
}
