using System;
using System.Collections.Generic;
using System.Diagnostics;
using Eto.Forms;
using MyRhinoPlugin1.vesselsDigitalModels;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.Render.CustomRenderMeshes;
using Rhino.UI;


namespace MyRhinoPlugin1.commands
{
    public class BuildVesselModel : Rhino.Commands.Command
    {
        Mittelplate mittelplate;

        public BuildVesselModel()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
            mittelplate = new Mittelplate();
        }
        public static BuildVesselModel Instance { get; private set; }


        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "BuildModel";

      

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {

            // Generate the 3D objects and perform Boolean operations
            Brep[] finalBrepResult = GenerateVesselGeometry(doc, mode);

            // Ensure the Boolean union succeeded
            if (finalBrepResult == null || finalBrepResult.Length == 0)
            {
                RhinoApp.WriteLine("Error: Boolean operations failed.");
                return Result.Failure;
            }

            // Create the "vesselConstruction" layer if it doesn't exist
            string layerName = "vesselConstruction";
            Layer vesselLayer = GetOrCreateLayer(doc, layerName);
            // Set the layer visibility to off
            vesselLayer.IsVisible = true;
            vesselLayer.IsLocked = true; 
            //doc.Layers.Modify(vesselLayer, vesselLayer.Index, true);

            // Add the final unioned Brep(s) to the document, assign them to the new layer
            foreach (Brep brep in finalBrepResult)
            {
                // Add the Brep to the document and get the Guid of the object
                Guid objGuid = doc.Objects.AddBrep(brep);

                // Get the RhinoObject associated with the Guid
                RhinoObject obj = doc.Objects.Find(objGuid);

                if (obj != null)
                {
                    // Assign the Brep to the "vesselConstruction" layer
                    obj.Attributes.LayerIndex = vesselLayer.Index;

                    // Commit changes to the object
                    obj.CommitChanges();
                }
            }

            foreach (Brep brep in mittelplate.TDs)
            {
                doc.Objects.AddBrep(brep);   
            }

                doc.Views.Redraw();

            foreach (var v in doc.Views.GetViewList(true, false))
            {
                v.ActiveViewport.ZoomExtents();
                v.Redraw();
            }

          
            return Result.Success;
        }

        // Method to get or create the "vesselConstruction" layer
        private Layer GetOrCreateLayer(RhinoDoc doc, string layerName)
        {
            // Search for the existing layer by name
            Layer layer = doc.Layers.FindName(layerName);

            if (layer == null)
            {
                // If the layer doesn't exist, create a new one
                layer = new Layer { Name = layerName };
                doc.Layers.Add(layer);
            }

            return layer;
        }


        // Method to generate vessel geometry (Brep) and perform Boolean difference
        private Brep[] GenerateVesselGeometry(RhinoDoc doc, RunMode mode)
        {
            // Generate the 3D objects
            Box vesselHold = drawVesselsHold();
            Box vesselBody = drawVessel();

            // Convert to Breps for Boolean operations
            Brep vesselHoldBrep = vesselHold.ToBrep();
            Brep vesselBodyBrep = vesselBody.ToBrep();

            // Ensure both Breps are valid
            if (vesselHoldBrep == null || vesselBodyBrep == null)
            {
                RhinoApp.WriteLine("Error: Invalid vessel geometry.");
                return null;
            }

            // Perform Boolean Difference: vesselBody - vesselHold
            Brep[] differenceResult = PerformBooleanDifference(vesselBodyBrep, vesselHoldBrep, doc);

            // Return null if the difference operation failed
            if (differenceResult == null || differenceResult.Length == 0)
            {
                RhinoApp.WriteLine("Error: Boolean Difference failed.");
                return null;
            }

            // Collect all Breps: Boolean Difference result + Vessel Elements
            List<Brep> allBreps = new List<Brep>(differenceResult);
            allBreps.AddRange(mittelplate.VesselElements);

            // Perform Boolean Union on all collected Breps
            return PerformBooleanUnion(allBreps, doc);
        }

        // Method to perform the Boolean Difference between two Breps
        private Brep[] PerformBooleanDifference(Brep vesselBodyBrep, Brep vesselHoldBrep, RhinoDoc doc)
        {
            return Brep.CreateBooleanDifference(vesselBodyBrep, vesselHoldBrep, doc.ModelAbsoluteTolerance);
        }

        // Method to perform the Boolean Union on a collection of Breps
        private Brep[] PerformBooleanUnion(List<Brep> allBreps, RhinoDoc doc)
        {
            Brep[] unionResult = Brep.CreateBooleanUnion(allBreps.ToArray(), doc.ModelAbsoluteTolerance);

            // Handle cases where Boolean Union fails
            if (unionResult == null || unionResult.Length == 0)
            {
                RhinoApp.WriteLine("Error: Boolean Union failed.");
                return null;
            }

            return unionResult;
        }

        private Box drawVesselsHold()
        {
            // Define base corner point (Cargo Hold Bottom-Front-Center)
            Point3d baseOrigin = mittelplate.CargoHoldBasePont;

            // Define box dimensions
            double length = mittelplate.CargoHoldLength;
            double width = mittelplate.CargoHoldWidth;
            double height = mittelplate.CargoHoldHeight;

            // Calculate bounding box corners
            Point3d minPoint = new Point3d(baseOrigin.X, baseOrigin.Y - width / 2, baseOrigin.Z);
            Point3d maxPoint = new Point3d(baseOrigin.X + length, baseOrigin.Y + width / 2, baseOrigin.Z + height);

            // Create a box
            return new Box(new BoundingBox(minPoint, maxPoint));

        }

        private Box drawVessel()
        {

            Point3d baseOrigin = mittelplate.APPoint;

            // Define box dimensions
            double length = mittelplate.VesselsLengthOA;
            double width = mittelplate.VesselsBreadth;
            double height = mittelplate.CargoHoldHeight + mittelplate.CargoHoldBasePont.Z;

            // Calculate bounding box corners
            Point3d minPoint = new Point3d(baseOrigin.X, baseOrigin.Y - width / 2, baseOrigin.Z);
            Point3d maxPoint = new Point3d(baseOrigin.X + length, baseOrigin.Y + width / 2, baseOrigin.Z + height);

            // Create a box
            return new Box(new BoundingBox(minPoint, maxPoint));

        }
    }
}


