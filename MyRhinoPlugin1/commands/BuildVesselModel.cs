using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using MyRhinoPlugin1.data;
using MyRhinoPlugin1.models;
using MyRhinoPlugin1.controllers;
using MyRhinoPlugin1.vesselsDigitalModels;
using MyRhinoPlugin1.behavior.collision;
using MyRhinoPlugin1.behavior.gravity;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;

using Rhino.UI;
using MyRhinoPlugin1.controllers.importController;
using System.ComponentModel;



namespace MyRhinoPlugin1.commands
{
    public class BuildVesselModel : Command
    {
        Mittelplate mittelplate;
        Layer vesselConstructionLayer;

        public BuildVesselModel()
        {
            Instance = this;
            mittelplate = new Mittelplate();
        }

        public static BuildVesselModel Instance { get; private set; }


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
            string layerName = "vesselConstruction";
             vesselConstructionLayer = service.LayerService.GetOrCreateLayer(doc, layerName);
            // Set the layer visibility to off
         

            // Add the final unioned Brep(s) to the document, assign them to the new layer
            foreach (Brep brep in finalBrepResult)
            {
                // Add the Brep to the document and get the Guid of the object
                Guid objGuid = doc.Objects.AddBrep(brep);

                // Get the RhinoObject associated with the Guid
                RhinoObject objTemp = doc.Objects.Find(objGuid);

                if (objTemp != null)
                {
                    Layer vesselConstructionTemp = doc.Layers.FindName("vesselConstruction");
                    // Assign the Brep to the "vesselConstruction" layer
                    objTemp.Attributes.LayerIndex = vesselConstructionTemp.Index;
                    objTemp.Attributes.Name = "vesselConstruction";
                    vesselConstructionTemp.IsLocked = true;
                    // Commit changes to the object
                    objTemp.CommitChanges();
                }
            }


            List<InstanceDefinition> blocksViews =  drawBlockViews(doc);

            foreach (InstanceDefinition block in blocksViews)
            {

                Guid objGuid = doc.Objects.AddInstanceObject(block.Index, Transform.Identity);
                RhinoObject objTemp = doc.Objects.Find(objGuid);
                if (objTemp != null)
                {
                    Layer vesselConstructionTemp = doc.Layers.FindName("vesselConstruction");
                    // Assign the Brep to the "vesselConstruction" layer
                    objTemp.Attributes.LayerIndex = vesselConstructionTemp.Index;
                    objTemp.Attributes.Name = "vesselConstruction";
                    vesselConstructionTemp.IsLocked = true;
                    // Commit changes to the object
                    objTemp.CommitChanges();
                }
            }



            // Lock the layer AFTER all objects are assigned
            vesselConstructionLayer.IsVisible = true;
            vesselConstructionLayer.IsLocked = false;

            // Create the "TD" layer if it doesn't exist
            string layerTDName = "TD";

   
            Layer vesselTDLayer = service.LayerService.GetOrCreateLayer(doc, layerTDName);
            // Set the layer visibility to off
            vesselTDLayer.IsVisible = true;
            vesselTDLayer.IsLocked = true;
            List<TDModel> TDList = new List<TDModel>();
            TDObjectCreator tDObjectCreator = new TDObjectCreator();
            TDList = tDObjectCreator.TDBrepCreator(mittelplate.TDList);

            foreach (var tD in TDList)
            {
                // Add the Brep to the document and get the Guid of the object
                Guid objGuid = doc.Objects.AddBrep(tD.TDModelBrep);
                // Get the RhinoObject associated with the Guid
                RhinoObject objTemp = doc.Objects.Find(objGuid);
                if (objTemp != null)
                {
                    // Get the layer index of the "TD" layer
                    Layer vesselTDLayerTemp = doc.Layers.FindName("TD");
                    // Assign the Brep to the "TD" layer
                    objTemp.Attributes.LayerIndex = vesselTDLayerTemp.Index;
                    objTemp.Attributes.Name = tD.Name;
                    mittelplate.TDInitialPositionPointList.Add(tD.Name, tD.LocationOfPosition);
                    // Commit changes to the object
                    objTemp.CommitChanges();
                    doc.Views.Redraw();
                    RhinoDoc.ActiveDoc.Views.Redraw();


                  
                }
            }


            


            // Set the singleton instance of DataModelHolder
            DataModelHolder.Instance.Vessel = mittelplate;
 
            doc.Views.Redraw();

            foreach (var v in doc.Views.GetViewList(true, false))
            {
                v.ActiveViewport.ZoomExtents();
                v.Redraw();
            }



            CollisionGuard.Enable();
            GravityWatcher.Enable();
            userInterface.CustomRhinoToolsBarInterface.CustomAllPannels(doc);
        
            return Result.Success;
        }



        private List<InstanceDefinition> drawBlockViews(RhinoDoc doc)
        {
            var resultList = new List<InstanceDefinition>();

            string viewFileName = "viewsMittelplateBlock.3dm";

            var blockNames = new[]
            {
        "sideViewMittelplateBlock",
        "topViewMittelplateBlock",
        "FwdViewMittelplateBlock"
    };
            

            foreach (var blockName in blockNames)
            {
                var block = OpenFilesWithBlockController.OpenFilesWithBlock(doc, viewFileName, blockName);
                if (block != null)
                { 
                    resultList.Add(block);
                
                   
                   
                }
                else
                {
                    RhinoApp.WriteLine($"Failed to load block: {blockName}");
                }
            }

            return resultList;
        }


        // Method to generate vessel geometry (Brep) and perform Boolean difference
        private Brep[] GenerateVesselGeometry(RhinoDoc doc, RunMode mode)
        {

            Brep vesselBodyBrep = drawBaseBox();
            Brep vesselHoldBrep = drawVesselsHold();




            // Ensure both Breps are valid
            if (vesselHoldBrep == null || vesselBodyBrep == null)
            {
                RhinoApp.WriteLine("Error: GenerateVesselGeometry => Invalid vessel geometry.");
                return null;
            }


            // Perform Boolean Difference: vesselBody - vesselHold
            Brep[] differenceResult = service.Operations3D.BooleanDifferenceOperations.PerformBooleanDifference(vesselBodyBrep, vesselHoldBrep, doc);
            // Check if the difference was successful
            if (differenceResult == null || differenceResult.Length == 0)
            {
                RhinoApp.WriteLine("Error: Boolean difference failed.");
                return null;
            }
            // add here Brep mittelplate.FuelTankPS + mittelplate.FuelTankSB + mittelplate.FWRObliquePS +  mittelplate.FWRObliqueSB
            var additionalParts = new List<Brep>
            {
                mittelplate.FuelTankPS,
                mittelplate.FuelTankSB,
                mittelplate.FWRObliquePS,
                mittelplate.FWRObliqueSB
            };
            // Combine with difference result
            additionalParts.AddRange(differenceResult);


            // Perform Boolean Union 
            Brep[] finalUnion = service.Operations3D.BooleanUnionOperations.PerformBooleanUnion(additionalParts, doc);

            if (finalUnion == null || finalUnion.Length == 0)
            {
                RhinoApp.WriteLine("Error: Boolean union failed.");
                return null;
            }

            // Return the final unioned Brep(s)
            return finalUnion;
        }


        private Brep drawBaseBox()
        {

            double length = mittelplate.CargoHoldLength + 15000;
            double width = mittelplate.VesselsHollBreadth;
            double height = mittelplate.VesselsHollHeight;
            // Calculate bounding box corners
            Point3d minPoint = new Point3d(mittelplate.CargoHoldBasePont.X - 1000, -width / 2, 0);
            Point3d maxPoint = new Point3d(length, width / 2, height);
            // Create a box
            Box startBoxVessel = new Box(new BoundingBox(minPoint, maxPoint));
            // Create a Brep from the box
            Brep startBoxVesselBrep = startBoxVessel.ToBrep();
            // Check if the Brep is valid
            if (startBoxVesselBrep == null || !startBoxVesselBrep.IsValid)
            {
                RhinoApp.WriteLine("Error: Failed to create a valid Brep from the box.");
                return null;
            }
            return startBoxVesselBrep;
        }


        private Brep drawVesselsHold()
        {
            // Define base corner point (Cargo Hold Bottom-Front-Center)
            Point3d baseOrigin = mittelplate.CargoHoldBasePont;

            // Define box dimensions
            double length = mittelplate.CargoHoldLength;
            double width = mittelplate.CargoHoldWidth;
            double height = mittelplate.CargoHoldHeight + 100;

            // Calculate bounding box corners
            Point3d minPoint = new Point3d(baseOrigin.X, baseOrigin.Y - width / 2, baseOrigin.Z);
            Point3d maxPoint = new Point3d(baseOrigin.X + length, baseOrigin.Y + width / 2, baseOrigin.Z + height);

            // Create a box

            return new Box(new BoundingBox(minPoint, maxPoint)).ToBrep();

        }
    }
}


