using MyRhinoPlugin1.controllers;
using MyRhinoPlugin1.models;
using MyRhinoPlugin1.service;
using MyRhinoPlugin1.userInterface;
using MyRhinoPlugin1.data;

using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Render;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace MyRhinoPlugin1.commands.layout
{
    public class LayoutMaker : Command
    {

        private static TextLayoutEditor _formInstance;
        private string frameBlockName = "frameBrieseChartering";
        private string viewFileName = "frameBrieseChartering.3dm";
        private string frameLayerName = "frameBrieseChartering";
        public Dictionary<String, String>  textLoyoutAttributes { get; private set; }

        public LayoutMaker()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }
        public static LayoutMaker Instance { get; private set; }
        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "LayoutMaker";
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Convert A4 size from millimeters to document units
            double width_mm = 297.0;
            double height_mm = 210.0;
            double width = RhinoMath.UnitScale(UnitSystem.Millimeters, doc.ModelUnitSystem) * width_mm;
            double height = RhinoMath.UnitScale(UnitSystem.Millimeters, doc.ModelUnitSystem) * height_mm;

            // Create the layout (PageView)
            string layoutName = "Stowage plan";
            var pageView = doc.Views.AddPageView(layoutName, width, height);
            if (pageView == null)
            {
                RhinoApp.WriteLine("Failed to create the layout.");
                return Result.Failure;
            }


            InstanceDefinition frame = OpenFilesWithBlockController.OpenFilesWithBlock(doc, viewFileName, frameBlockName);
            if (frame == null)
            {
                RhinoApp.WriteLine($"Block '{frameBlockName}' not found in file '{viewFileName}'.");
                return Result.Failure;
            }
            Layer frameLayer = service.LayerService.GetOrCreateLayer(doc, frameLayerName);
            frameLayer.IsLocked = true;
            Guid objGuid = doc.Objects.AddInstanceObject(frame.Index, Transform.Identity);
            RhinoObject objTemp = doc.Objects.Find(objGuid);
            if (objTemp != null)
            {
                Layer layerTemp = doc.Layers.FindName(frameLayer.Name);
                objTemp.Attributes.LayerIndex = layerTemp.Index;
                objTemp.Attributes.Name = frameLayer.Name;
                layerTemp.IsLocked = true;
                objTemp.CommitChanges();
                Transform transform = Transform.Translation(new Vector3d(0, 0, 0));
                doc.Objects.Transform(objGuid, transform, true); // true to update the bounding box

            }
            int tip = 2;
            int XLeftoutreach = 5;
            int YLeftUpStart = 205;


            int ViewLength = 215;
            int ViewheightSide = 45;
            int ViewheightTop = 35;




            // start /////////////////////////////////////////////

            string layerToHideVesselConstruction = "vesselConstruction";
            string layerToHideTopViewMittelplateBlock = "topViewMittelplateBlock";
            string layerToHideFwdViewMittelplateBlock = "FwdViewMittelplateBlock";
            string layerToHideSideViewMittelplateBlock = "sideViewMittelplateBlock";

            var layerVesselConstruction = doc.Layers.FindName(layerToHideVesselConstruction);
            var layerTopViewMittelplateBlock = doc.Layers.FindName(layerToHideTopViewMittelplateBlock);
            var layerFwdViewMittelplateBlock = doc.Layers.FindName(layerToHideFwdViewMittelplateBlock);
            var layerSideViewMittelplateBlock = doc.Layers.FindName(layerToHideSideViewMittelplateBlock);


            ///////////////////////////////////////////// SIDE /////////////////////////////////////////////
            Point2d pointASide = new Point2d(XLeftoutreach, YLeftUpStart);
            Point2d pointBSide = new Point2d(XLeftoutreach+ViewLength, YLeftUpStart- ViewheightSide);
            var detailSide = pageView.AddDetailView("side view", pointASide, pointBSide, DefinedViewportProjection.Front);
            if (detailSide != null)
            { 

                layerVesselConstruction.SetPerViewportVisible(detailSide.Id, false);
                layerTopViewMittelplateBlock.SetPerViewportVisible(detailSide.Id, false);
                layerFwdViewMittelplateBlock.SetPerViewportVisible(detailSide.Id, false);

                

                var attr = detailSide.Attributes;
                attr.ObjectColor = System.Drawing.Color.White;
                attr.ColorSource = ObjectColorSource.ColorFromObject;
                attr.Name = "side view";
                 
                doc.Objects.ModifyAttributes(detailSide.Id, attr, true);
                detailSide.Viewport.DisplayMode = DisplayModeDescription.FindByName("Shaded");
                
                detailSide.CommitViewportChanges();  // Commits viewport-related changes (like zoom, mode)
                detailSide.CommitChanges();          // Commits object-level changes
            }


            ///////////////////////////////////////////// WD /////////////////////////////////////////////
            Point2d pointATopWD = new Point2d(XLeftoutreach, pointBSide.Y - tip);
            Point2d pointBTopWD = new Point2d(XLeftoutreach + ViewLength, pointBSide.Y-tip- ViewheightTop);
            var detailWD = pageView.AddDetailView("WD view", pointATopWD, pointBTopWD, DefinedViewportProjection.Top);

            if (detailWD != null)
            {
                layerVesselConstruction.SetPerViewportVisible(detailWD.Id, false);
                layerSideViewMittelplateBlock.SetPerViewportVisible(detailWD.Id, false);
                layerFwdViewMittelplateBlock.SetPerViewportVisible(detailWD.Id, false);

             


                var attr = detailWD.Attributes;
                attr.Name = "WD view";
                attr.ObjectColor = System.Drawing.Color.White;
                attr.ColorSource = ObjectColorSource.ColorFromObject;
                doc.Objects.ModifyAttributes(detailWD.Id, attr, true);
                detailWD.Viewport.DisplayMode = DisplayModeDescription.FindByName("Shaded");
                detailWD.CommitViewportChanges();  // Commits viewport-related changes (like zoom, mode)
                detailWD.CommitChanges();          // Commits object-level changes
            }


            ///////////////////////////////////////////// TD /////////////////////////////////////////////
            Point2d pointATopTD = new Point2d(XLeftoutreach, pointBTopWD.Y - tip);
            Point2d pointBTopTD = new Point2d(XLeftoutreach + ViewLength, pointBTopWD.Y - tip - ViewheightTop);
            var detailTD = pageView.AddDetailView("TD view", pointATopTD, pointBTopTD, DefinedViewportProjection.Top);
            if (detailTD != null)
            {
                layerVesselConstruction.SetPerViewportVisible(detailTD.Id, false);
                layerSideViewMittelplateBlock.SetPerViewportVisible(detailTD.Id, false);
                layerFwdViewMittelplateBlock.SetPerViewportVisible(detailTD.Id, false);

               
                var attr = detailTD.Attributes;
                attr.Name = "TD view";
                attr.ObjectColor = System.Drawing.Color.White;
                attr.ColorSource = ObjectColorSource.ColorFromObject;
                doc.Objects.ModifyAttributes(detailTD.Id, attr, true);
                detailTD.Viewport.DisplayMode = DisplayModeDescription.FindByName("Shaded");
                detailTD.CommitViewportChanges();  // Commits viewport-related changes (like zoom, mode)
                detailTD.CommitChanges();          // Commits object-level changes
            }


            ///////////////////////////////////////////// TT /////////////////////////////////////////////
            Point2d pointATopTT = new Point2d(XLeftoutreach, pointBTopTD.Y - tip);
            Point2d pointBTopTT = new Point2d(XLeftoutreach + ViewLength, pointBTopTD.Y - tip - ViewheightTop);
            var detailTT = pageView.AddDetailView("TT view", pointATopTT, pointBTopTT, DefinedViewportProjection.Top);
            if (detailTT != null)
            {
                layerVesselConstruction.SetPerViewportVisible(detailTT.Id, false);
                layerSideViewMittelplateBlock.SetPerViewportVisible(detailTT.Id, false);
                layerFwdViewMittelplateBlock.SetPerViewportVisible(detailTT.Id, false);

             
                pageView.SetActiveDetail(detailTT.Id); // 🔥 Activate the detail
                double z = 4000;
                Plane plane = new Plane(new Point3d(55000, 0, z), new Vector3d(0, 0, -100));
                if (!plane.IsValid)
                {
                    RhinoApp.WriteLine("Plane is invalid");
                }
                var planeWidth = 5000;
                var planeHeight = 5000;
                // Add clipping plane object
                Guid clippingPlaneId = doc.Objects.AddClippingPlane(plane, planeWidth, planeHeight, new List<Guid> { detailTT.Viewport.Id });

                // Associate it with your layout detail
                if (clippingPlaneId != Guid.Empty)
                {
                    // Retrieve using FindId instead of Find
                    var rhClippingPlaneObj = doc.Objects.FindId(clippingPlaneId) as ClippingPlaneObject;

                    if (rhClippingPlaneObj != null)
                    {
                        var attr = detailTT.Attributes;
                        attr.Name = "TT view";
                        attr.ObjectColor = System.Drawing.Color.White;
                        attr.ColorSource = ObjectColorSource.ColorFromObject;
                        doc.Objects.ModifyAttributes(detailTT.Id, attr, true);
                 
                        detailTT.Viewport.DisplayMode = DisplayModeDescription.FindByName("Shaded");
                        rhClippingPlaneObj.AddClipViewport(detailTT.Viewport, true); 
                        doc.Objects.Select(clippingPlaneId);
                        doc.Views.Redraw();
                        doc.Objects.UnselectAll();
                        rhClippingPlaneObj.CommitChanges();
                        detailTT.CommitViewportChanges();      // ✅ Commit changes
                        //pageView.SetPageAsActive();            // (Optional) Return to layout
                     
                        RhinoApp.WriteLine("Added and activated clipping plane for TT view.");
                      
                    }
                    else
                    {
                        RhinoApp.WriteLine("rhino ClippingPlane Object was not found.");
                    }
                  
                }
                else
                {
                    RhinoApp.WriteLine("Clipping plane ID object was not found.");
                } 
       
            }


            ///////////////////////////////////////////// FWR ///////////////////////////////////////////////
            Point2d pointAFWD = new Point2d(pointBSide.X + tip, YLeftUpStart);
            Point2d pointBFWD = new Point2d(pointBSide.X + tip + ViewLength/4, YLeftUpStart - ViewheightSide);
            var detailFWR =  pageView.AddDetailView("fwr view", pointAFWD, pointBFWD, DefinedViewportProjection.Left);

            if (detailFWR != null)
            {
                layerVesselConstruction.SetPerViewportVisible(detailFWR.Id, false);
                layerSideViewMittelplateBlock.SetPerViewportVisible(detailFWR.Id, false);
                layerTopViewMittelplateBlock.SetPerViewportVisible(detailFWR.Id, false);

                var attr = detailFWR.Attributes;
                attr.Name = "fwr view";
                attr.ObjectColor = System.Drawing.Color.White;
                attr.ColorSource = ObjectColorSource.ColorFromObject;
                doc.Objects.ModifyAttributes(detailFWR.Id, attr, true);
                detailFWR.Viewport.DisplayMode = DisplayModeDescription.FindByName("Shaded");
                detailFWR.CommitViewportChanges();  // Commits viewport-related changes (like zoom, mode)
                detailFWR.CommitChanges();          // Commits object-level changes



                
            }

            if (_formInstance == null || !_formInstance.Visible)
            {
                _formInstance = new TextLayoutEditor();
                _formInstance.Closed += (s, e) => _formInstance = null; // Clear ref on close
                _formInstance.Show(); // Use ShowModal() if blocking is needed

            }
            else
            {
                RhinoApp.WriteLine("The UI is already open.");
            }





            // Refresh the layout view 
            textUpdator(doc);
          
            return Result.Success;
        }



        public static void textUpdator(RhinoDoc doc)
        {
            var objsToDelete = doc.Objects
    .Where(obj => obj.Geometry is TextEntity && obj.Attributes.LayerIndex == doc.Layers.FindByFullPath("textLayerName", true))
    .Select(obj => obj.Id)
    .ToList();

            foreach (var id in objsToDelete)
            {
                doc.Objects.Delete(id, quiet: true);
            }

            TextOnDrawingsMaker.AddLayoutText(doc, "Vessel name", new Point3d(230, 44, 0), 2, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, "Vessel type", new Point3d(230, 39, 0), 2, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, "plan", new Point3d(230, 33, 0), 2, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, "status", new Point3d(230, 26, 0), 2, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, "planner", new Point3d(230, 20, 0), 2, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, "job ID", new Point3d(230, 14, 0), 2, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, "date", new Point3d(230, 8, 0), 2, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, "rev", new Point3d(283, 14, 0), 2, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, "project", new Point3d(135, 14, 0), 2, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, "remarks", new Point3d(135, 8, 0), 2, "textLayerName");

            TextOnDrawingsMaker.AddLayoutText(doc, DataModelHolder.CompamnyName, new Point3d(235, 49, 0), 4, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, DataModelHolder.VesselName, new Point3d(250, 43, 0), 3, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, DataModelHolder.VesselType, new Point3d(250, 37, 0), 3, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, DataModelHolder.planType, new Point3d(250, 32, 0), 3, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, DataModelHolder.planStatus, new Point3d(250, 25, 0), 3, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, DataModelHolder.planner, new Point3d(250, 19, 0), 3, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, DataModelHolder.jobId, new Point3d(250, 13, 0), 3, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, DataModelHolder.Date, new Point3d(250, 7, 0), 3, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, DataModelHolder.rev, new Point3d(285, 10, 0), 3, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, DataModelHolder.titel, new Point3d(165, 13, 0), 3, "textLayerName");
            TextOnDrawingsMaker.AddLayoutText(doc, DataModelHolder.subject, new Point3d(165, 7, 0), 3, "textLayerName");
        }

    }
}
