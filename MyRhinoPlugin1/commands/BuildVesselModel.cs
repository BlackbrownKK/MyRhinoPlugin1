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
using MyRhinoPlugin1.behavior.visualView;
using Rhino.UI;
using MyRhinoPlugin1.controllers.importController;
using System.ComponentModel;
using Rhino.Render.CustomRenderMeshes;
using Rhino.Display;



namespace MyRhinoPlugin1.commands
{
    public class BuildVesselModel : Command
    {
        Mittelplate mittelplate;
        Layer vesselConstructionLayer;
        String sideViewMittelplateBlockName = "sideViewMittelplateBlock";
        String topViewMittelplateBlockName = "topViewMittelplateBlock";
        String FwdViewMittelplateBlockName = "FwdViewMittelplateBlock";
        private static CrossSectionMouseListener _mouseListener;
        public BuildVesselModel()
        {
            Instance = this;
            mittelplate = new Mittelplate();
        }

        public static BuildVesselModel Instance { get; private set; }


        public override string EnglishName => "BuildModel";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Check if the vessel is loaded in the singleton
            if (DataModelHolder.Instance.Vessel != null)
            {
                RhinoApp.WriteLine("Vessel is already loaded in the data model.");
                return Result.Failure;
            }

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
            vesselConstructionLayer.ModelIsVisible = false; // Set the layer visibility to off
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
                    vesselConstructionTemp.ModelIsVisible = false;
                    doc.Views.Redraw();
                    objTemp.CommitChanges();
                }
            }


           drawBlockViews(doc);
           

            // Create the "TD" layer if it doesn't exist
            string layerTDName = "TD";

   
            Layer vesselTDLayer = service.LayerService.GetOrCreateLayer(doc, layerTDName);
            // Set the layer visibility to off
            vesselTDLayer.IsVisible = true;
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

            
           

            foreach (var v in doc.Views.GetViewList(true, false))
            {
                v.ActiveViewport.ZoomExtents();
                v.Redraw();
            }



            CollisionGuard.Enable();
            GravityWatcher.Enable();
            userInterface.CustomRhinoToolsBarInterface.CustomAllPannels(doc);
            var topView = doc.Views.Find(CustomViewportLayoutCommand.TopViewName, false);
            var sideView = doc.Views.Find(CustomViewportLayoutCommand.SideViewName, false);
            var fwdView = doc.Views.Find(CustomViewportLayoutCommand.FwdViewName, false);

            var layerSadeView = doc.Layers.FindName(sideViewMittelplateBlockName);
            layerSadeView.SetPerViewportVisible(topView.ActiveViewportID, false);
            layerSadeView.SetPerViewportVisible(fwdView.ActiveViewportID, false);

            var layerFwdView = doc.Layers.FindName(FwdViewMittelplateBlockName);
            layerFwdView.SetPerViewportVisible(sideView.ActiveViewportID, false);
            layerFwdView.SetPerViewportVisible(topView.ActiveViewportID, false);

            var layerTopView = doc.Layers.FindName(topViewMittelplateBlockName);
            layerTopView.SetPerViewportVisible(sideView.ActiveViewportID, false);
            layerTopView.SetPerViewportVisible(fwdView.ActiveViewportID, false);


            _mouseListener = new CrossSectionMouseListener();
            _mouseListener.Enabled = true;
            // Set the singleton instance of DataModelHolder
            DataModelHolder.Instance.Vessel = mittelplate;
            doc.Views.Redraw();
            return Result.Success;
        }



        private void drawBlockViews(RhinoDoc doc)
        {
            string viewFileName = "viewsMittelplateBlock.3dm";
            Layer sideBlockViewsLayer = service.LayerService.GetOrCreateLayer(doc, sideViewMittelplateBlockName);
            sideBlockViewsLayer.IsLocked = true;  
            Layer topBlockViewsLayer = service.LayerService.GetOrCreateLayer(doc, topViewMittelplateBlockName);
            topBlockViewsLayer.IsLocked = true;
            Layer fwdBlockViewsLayer = service.LayerService.GetOrCreateLayer(doc, FwdViewMittelplateBlockName);
            fwdBlockViewsLayer.IsLocked = true;
            Dictionary <Layer, InstanceDefinition > blocksViews = new Dictionary<Layer, InstanceDefinition>
            {
                { sideBlockViewsLayer, OpenFilesWithBlockController.OpenFilesWithBlock(doc, viewFileName, sideViewMittelplateBlockName) },
                { topBlockViewsLayer, OpenFilesWithBlockController.OpenFilesWithBlock(doc, viewFileName, topViewMittelplateBlockName) },
                { fwdBlockViewsLayer, OpenFilesWithBlockController.OpenFilesWithBlock(doc, viewFileName, FwdViewMittelplateBlockName) }
            };

            foreach(Layer layer in blocksViews.Keys)
            {
                InstanceDefinition tempBlock = blocksViews[layer];
                Guid objGuid = doc.Objects.AddInstanceObject(tempBlock.Index, Transform.Identity);
                RhinoObject objTemp = doc.Objects.Find(objGuid);
                if (objTemp != null)
                {    
                    Layer layerTemp = doc.Layers.FindName(layer.Name);
                    // Assign the Brep to the "vesselConstruction" layer
                    objTemp.Attributes.LayerIndex = layerTemp.Index;
                    objTemp.Attributes.Name = layer.Name;
                    layerTemp.IsLocked = true;
                    objTemp.CommitChanges();



                    if (layer.Name.Equals(sideViewMittelplateBlockName))
                    {
                        // move the block to the right 
                        Transform moveRight = Transform.Translation(new Vector3d(0, mittelplate.VesselsHollBreadth / 2, 0));
                        doc.Objects.Transform(objGuid, moveRight, true); // `true` = delete original
                    }
                     
                } 
            } 
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
            Brep startBoxVesselBrep = startBoxVessel.ToBrep();

            if (startBoxVesselBrep == null || !startBoxVesselBrep.IsValid)
            {
                RhinoApp.WriteLine("Error: Failed to create a valid Brep from the box.");
                return null;
            }

            double tolerance = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            var edgesToFillet = new List<int>();

            foreach (var edge in startBoxVesselBrep.Edges)
            {
                var curve = edge.EdgeCurve;
                var start = curve.PointAtStart;
                var end = curve.PointAtEnd;

                bool isAtBottom = Math.Abs(start.Z) < tolerance && Math.Abs(end.Z) < tolerance;
                bool isLongitudinal = Math.Abs(start.Y - end.Y) < tolerance;

                if (isAtBottom && isLongitudinal)
                {
                    edgesToFillet.Add(edge.EdgeIndex);
                }
            }

            double filletRadius = 1000; // Adjust this radius as needed
            var filleted = Brep.CreateFilletEdges(
                startBoxVesselBrep,
                edgesToFillet,
                Enumerable.Repeat(filletRadius, edgesToFillet.Count),
                Enumerable.Repeat(filletRadius, edgesToFillet.Count),
                BlendType.Fillet,
                RailType.RollingBall,
                tolerance
            );

            return filleted?.FirstOrDefault() ?? startBoxVesselBrep;
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


