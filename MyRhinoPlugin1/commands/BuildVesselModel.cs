using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using MyRhinoPlugin1.data;
using MyRhinoPlugin1.models;
using MyRhinoPlugin1.controllers;
using MyRhinoPlugin1.vesselsDigitalModels;
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
            Layer vesselLayer = service.LayerService.GetOrCreateLayer(doc, layerName);
            // Set the layer visibility to off
            vesselLayer.IsVisible = true;
            vesselLayer.IsLocked = false;

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

            // Create the "TD" layer if it doesn't exist
            string layerTDName = "TD";

            /* Unmerged change from project 'MyRhinoPlugin1 (net7.0)'
            Before:
                        Layer vesselTDLayer = utilites.LayerService.GetOrCreateLayer(doc, layerTDName);
                        // Set the layer visibility to off
            After:
                        Layer vesselTDLayer = LayerService.GetOrCreateLayer(doc, layerTDName);
                        // Set the layer visibility to off
            */
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


            drawBlockViews(doc);


            // Set the singleton instance of DataModelHolder
            DataModelHolder.Instance.Vessel = mittelplate;
 
            doc.Views.Redraw();

            foreach (var v in doc.Views.GetViewList(true, false))
            {
                v.ActiveViewport.ZoomExtents();
                v.Redraw();
            }



            behavior.collision.CollisionGuard.Enable();
            behavior.gravity.GravityWatcher.Enable();
            userInterface.CustomRhinoToolsBarInterface.CustomAllPannels(doc);
        
            return Result.Success;
        }

        private void drawBlockViews(RhinoDoc doc)
        { 
            string sideViewFileName = "sideViewMittelplateBlock.3dm";

            string sideViewBlockName = "sideViewMittelplateBlock";
            string topViewBlockName = "topViewMittelplateBlock";
            string fwdViewBlockName = "FwdViewMittelplateBlock"; 

            InstanceDefinition SideBlock  = OpenFilesWithBlockController.OpenFilesWithBlock(doc, sideViewFileName, sideViewBlockName);
           // Add the block to the document and get the Guid of the object 
            Guid objGuid = doc.Objects.AddInstanceObject(SideBlock.Index, Transform.Translation(0, 0, 0));
            // Get the RhinoObject associated with the Guid
            RhinoObject objTemp = doc.Objects.Find(objGuid);
            objTemp.Attributes.LayerIndex = doc.Layers.FindName("vesselConstruction").Index;
            objTemp.Attributes.Name = "sideViewMittelplateBlock";

            InstanceDefinition TopBlock = OpenFilesWithBlockController.OpenFilesWithBlock(doc, sideViewFileName, topViewBlockName);
            objGuid = doc.Objects.AddInstanceObject(TopBlock.Index, Transform.Translation(0, 0, 0));
            objTemp = doc.Objects.Find(objGuid);
            objTemp.Attributes.LayerIndex = doc.Layers.FindName("vesselConstruction").Index;
            objTemp.Attributes.Name = "sideViewMittelplateBlock";

            InstanceDefinition FwdView = OpenFilesWithBlockController.OpenFilesWithBlock(doc, sideViewFileName, fwdViewBlockName);
            objGuid = doc.Objects.AddInstanceObject(FwdView.Index, Transform.Translation(0, 0, 0));
            objTemp = doc.Objects.Find(objGuid);
            objTemp.Attributes.LayerIndex = doc.Layers.FindName("vesselConstruction").Index;
            objTemp.Attributes.Name = "sideViewMittelplateBlock";  
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


