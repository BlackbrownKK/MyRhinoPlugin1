using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino;
using System;
using System.Collections.Generic;
using MyRhinoPlugin1.models;
using System.Xml.Linq;
using MyRhinoPlugin1.controllers;
using Rhino.Display;

namespace MyRhinoPlugin1.commands.layout
{
    public class LayoutMaker : Command
    {


        private string frameBlockName = "frameBrieseChartering";
        private string viewFileName = "frameBrieseChartering.3dm";
        private string frameLayerName = "frameBrieseChartering";

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
            int XLeftoutreach = 10;
            int YLeftUpStart = 205;


            int ViewLength = 200;
            int Viewheight = 45;


            Point2d pointASide = new Point2d(XLeftoutreach, YLeftUpStart);
            Point2d pointBSide = new Point2d(XLeftoutreach+ViewLength, YLeftUpStart- Viewheight);
            //pageView.AddDetailView("side view", pointASide, pointBSide, DefinedViewportProjection.Front).Attributes.ComputedPlotColor(Color4f);
            pageView.AddDetailView("side view", pointASide, pointBSide, DefinedViewportProjection.Front);


            Point2d pointATopWD = new Point2d(XLeftoutreach, pointBSide.Y - tip);
            Point2d pointBTopWD = new Point2d(XLeftoutreach + ViewLength, pointBSide.Y-tip-Viewheight);
            pageView.AddDetailView("WD view", pointATopWD, pointBTopWD, DefinedViewportProjection.Top);

            Point2d pointATopTD = new Point2d(XLeftoutreach, pointBTopWD.Y - tip);
            Point2d pointBTopTD = new Point2d(XLeftoutreach + ViewLength, pointBTopWD.Y - tip - Viewheight);
            pageView.AddDetailView("TD view", pointATopTD, pointBTopTD, DefinedViewportProjection.Top);

            Point2d pointATopTT = new Point2d(XLeftoutreach, pointBTopTD.Y - tip);
            Point2d pointBTopTT = new Point2d(XLeftoutreach + ViewLength, pointBTopTD.Y - tip - Viewheight);
            pageView.AddDetailView("TT view", pointATopTT, pointBTopTT, DefinedViewportProjection.Top);


            //
            Point2d pointAFWD = new Point2d(pointBSide.X + tip, YLeftUpStart);
            Point2d pointBFWD = new Point2d(pointBSide.X + tip + ViewLength/4, YLeftUpStart - Viewheight);
            pageView.AddDetailView("fwr view", pointAFWD, pointBFWD, DefinedViewportProjection.Left);


            // Refresh the layout view 
            doc.Views.Redraw();
            RhinoApp.WriteLine($"Created layout '{layoutName}' with size {width_mm}mm x {height_mm}mm.");
            return Result.Success;
        }

        
    }
}
