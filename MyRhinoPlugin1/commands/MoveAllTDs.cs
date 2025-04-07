using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyRhinoPlugin1.vesselsDigitalModels;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace MyRhinoPlugin1.commands
{
    public class MoveAllTDs : Command

    {
        public override string EnglishName => "MoveAllTDToLowerPosition"; 
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Get the TD model from the document 
            Mittelplate mittelplate = new Mittelplate();

            Point3d InitialTSPosotion = new Point3d(
              mittelplate.CargoHoldBasePont.X + mittelplate.firstOffset,
              -mittelplate.CargoHoldWidth / 2,
              mittelplate.CargoHoldBasePont.Z + mittelplate.TDAltitudeLowerPosition);
  
            // Get all TD models from the document
              var tdModels = doc.Objects.FindByLayer("TD")
                .Where(obj => obj is BrepObject && obj.Name.StartsWith("TD"))
                .Select(obj => (BrepObject)obj)
                .ToList();

            // Move each TD model to its lower position
            foreach (var tdModel in tdModels)
            {
                // Get current geometry
                var brep = tdModel.Geometry as Brep;
                if (brep == null)
                    continue;

                // Get current Z (lowest point of bounding box)
                var bbox = brep.GetBoundingBox(true);
                double currentZ = bbox.Min.Z;

                // Try to parse TDAltitudeLowerPosition from the name or user string
                double targetZ = 0;

                // Option 1: If you've stored TDAltitudeLowerPosition as a user string:
                var lowerPosStr = tdModel.Attributes.GetUserString("TDAltitudeLowerPosition");
                if (!string.IsNullOrEmpty(lowerPosStr) && double.TryParse(lowerPosStr, out double parsedZ))
                {
                    targetZ = parsedZ;
                }
                else
                {
                    RhinoApp.WriteLine($"⚠️ TD object '{tdModel.Name}' missing TDAltitudeLowerPosition user string.");
                    continue;
                }

                // Compute translation vector
                double deltaZ = targetZ - currentZ;
                var translation = Transform.Translation(0, 0, deltaZ);

                // Transform object in doc
                doc.Objects.Transform(tdModel, translation, true);
            }
            doc.Views.Redraw();
            return Result.Success;
        }
    }
}
