using System;
using System.Collections.Generic;
using MyRhinoPlugin1.data;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace MyRhinoPlugin1.commands
{
    public class MoveAllTDtoLowPoss : Command

    {

        public static MoveAllTDtoLowPoss Instance { get; private set; }

     

        public MoveAllTDtoLowPoss()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a reference in a static property.
            Instance = this;
        }

        public override string EnglishName => "MoveAllTDToLowerPosition";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            double z = DataModelHolder.Instance.Vessel.CargoHoldBasePont.Z;
            // Check if the layer "TD" exists
            if (doc.Layers.FindName("TD") == null)
            {
                RhinoApp.WriteLine("Layer 'TD' does not exist.");
                return Result.Failure;
            }
            // set the z coordinate to the base point of the all TDs in TS list

            // Iterate through all objects in the document
            foreach (RhinoObject obj in doc.Objects)
            {
                // Check if the object is on the "TD" layer
                // if the object name is the one on the DataModelHolder.vessel.TDInitialPositionPointList => set the z coordinate to the base point of this obj to standat position - x, y, byt z is -> DataModelHolder.vessel.TDAltitudeLowerPosition
                // if the object name is the one on the DataModelHolder.vessel.TDInitialPositionPointList => set the z coordinate to the base point of this obj to standat position - x, y, byt z is -> DataModelHolder.vessel.TDAltitudeLowerPosition
 
                if (obj.Attributes.LayerIndex == doc.Layers.FindName("TD").Index && DataModelHolder.Instance.Vessel.TDInitialPositionPointList.ContainsKey(obj.Name))
                {
                    // Get the current position (e.g., centroid of bounding box)
                    BoundingBox bbox = obj.Geometry.GetBoundingBox(true);
                    Point3d currentBasePoint = new Point3d(bbox.Min.X, bbox.Min.Y, bbox.Min.Z);


                    Point3d adjustedPoint = new Point3d(
                        DataModelHolder.Instance.Vessel.TDInitialPositionPointList[obj.Name].X,
                        DataModelHolder.Instance.Vessel.TDInitialPositionPointList[obj.Name].Y, 
                        DataModelHolder.Instance.Vessel.TDAltitudeLowerPosition); // Set the Z coordinate to the desired value
  
                    // Get the geometry and move it
                    GeometryBase geo = obj.Geometry;
                    if (geo is Brep brep)
                    {
                        Vector3d moveVector = adjustedPoint - currentBasePoint;
                        Transform translation = Transform.Translation(moveVector);
                        brep.Transform(translation);

                        // Replace the object with the transformed geometry
                        doc.Objects.Replace(obj.Id, brep);
                    }

                    // Commit the changes to the object
                    obj.CommitChanges();
                }
            }
            doc.Views.Redraw();
            return Result.Success;
        }
    }
}

