using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using MyRhinoPlugin1.data;
using MyRhinoPlugin1.models;
using MyRhinoPlugin1.vesselsDigitalModels;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;



namespace MyRhinoPlugin1.commands
{
    public class BuildVesselModel : Command
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
            Layer vesselLayer = utilites.LayerService.GetOrCreateLayer(doc, layerName);
            // Set the layer visibility to off
            vesselLayer.IsVisible = true;
            vesselLayer.IsLocked = false;

            // Add the final unioned Brep(s) to the document, assign them to the new layer
            foreach (Brep brep in finalBrepResult)
            {
                // Add the Brep to the document and get the Guid of the object
                Guid objGuid = doc.Objects.AddBrep(brep);

                // Get the RhinoObject associated with the Guid
                RhinoObject obj = doc.Objects.Find(objGuid);

                if (obj != null)
                {
                    Layer vesselConstructionTemp = doc.Layers.FindName("vesselConstruction");

                    // Assign the Brep to the "vesselConstruction" layer
                    obj.Attributes.LayerIndex = vesselConstructionTemp.Index;
                    obj.Attributes.Name = "vesselConstruction";
                    vesselConstructionTemp.IsLocked = false;
                    // Commit changes to the object
                    obj.CommitChanges();
                }
            }




            foreach (var v in doc.Views.GetViewList(true, false))
            {
                v.ActiveViewport.ZoomExtents();
                v.Redraw();
            }


            // Create the "TD" layer if it doesn't exist
            string layerTDName = "TD";
            Layer vesselTDLayer = utilites.LayerService.GetOrCreateLayer(doc, layerTDName);
            // Set the layer visibility to off
            vesselTDLayer.IsVisible = true;
            vesselTDLayer.IsLocked = false;
            List<TDModel> TDList = new List<TDModel>();
            TDObjectCreator tDObjectCreator = new TDObjectCreator();
            TDList = tDObjectCreator.TDBrepCreator(mittelplate.TDList);

            foreach (var tD in TDList)
            {
                // Add the Brep to the document and get the Guid of the object
                Guid objGuid = doc.Objects.AddBrep(tD.TDModelBrep);
                // Get the RhinoObject associated with the Guid
                RhinoObject obj = doc.Objects.Find(objGuid);
                if (obj != null)
                {
                    // Get the layer index of the "TD" layer
                    Layer vesselTDLayerTemp = doc.Layers.FindName("TD");
                    // Assign the Brep to the "TD" layer
                    obj.Attributes.LayerIndex = vesselTDLayerTemp.Index;
                    obj.Attributes.Name = tD.Name;
                    mittelplate.TDInitialPositionPointList.Add(tD.Name, tD.LocationOfPosition);
                    // Commit changes to the object
                    obj.CommitChanges();
                    doc.Views.Redraw();
                    RhinoDoc.ActiveDoc.Views.Redraw();
                }
            }

            CustomViewportLayoutCommand.customViewsMaker(doc, mode);

            // Set the singleton instance of DataModelHolder

            DataModelHolder.Instance.Vessel = mittelplate;

            return Result.Success;

        }



        // Method to generate vessel geometry (Brep) and perform Boolean difference
        private Brep[] GenerateVesselGeometry(RhinoDoc doc, RunMode mode)
        {
            Brep vesselBodyBrep = drawVesselBase(doc);
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

            // Return the final unioned Brep(s)
            return differenceResult;
        }

        private Brep drawVesselBase(RhinoDoc doc)
        {
            string SideCurveMittelplateBody = "SideCurveMittelplateBody.txt";
            string TopCurveMittelplateBody = "TopCurveMittelplateBody.txt";
            Brep maxBaseBox = drawBaseBox();
            doc.Objects.AddBrep(maxBaseBox);

            Brep top = drawVesselBaseFromTopView(doc, TopCurveMittelplateBody);
            Brep side = drawVesselBaseFromSideView(doc, SideCurveMittelplateBody);
            // toto side - (maxBaseBox - top)
            Brep[] BoxMinusTop = service.Operations3D.BooleanDifferenceOperations.PerformBooleanDifference(maxBaseBox, top, doc);
             
           // doc.Objects.AddBrep(BoxMinusTop);
           // Brep sideMinusBoxMinusTop = service.Operations3D.BooleanDifferenceOperations.PerformBooleanDifference(side, BoxMinusTop, doc)[0];

            return side;
        }

        private Brep drawBaseBox()
        {
            double length = mittelplate.VesselsLengthOA;
            double width = mittelplate.VesselsBreadth;
            double height = mittelplate.VesselsHeight * 2;
            // Calculate bounding box corners
            Point3d minPoint = new Point3d(0, -width / 2, 0);
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



        // Method to draw the vessel body. Here creare
        private Curve drawVesselsCurve(string curveName, string filename)
        {

            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string path = Path.Combine(projectRoot, "vesselsDigitalModels", filename);
            RhinoApp.WriteLine(path);
            // Check if the file exists
            if (!File.Exists(path))
            {
                RhinoApp.WriteLine($"Error: File not found at {path}");
                return null;

            }
            List<Point3d> pointsCollection = service.ReadPointsFromTxt.PointsReader(path);
            service.CerveCreator curveCreator = new service.CerveCreator(curveName, pointsCollection);
            Curve curve = curveCreator.CreateSafeCurve();

            // Make sure curve is valid and closed
            if (curve == null || !curve.IsClosed)
            {
                RhinoApp.WriteLine("Curve is null or not closed.");
                return null;
            }
            return curve;
        }

        private Brep drawVesselBaseFromSideView(RhinoDoc doc, string pathFileName)
        {
            Curve sideCurve = drawVesselsCurve("sideCurve", pathFileName);  
            double offset = -mittelplate.VesselsBreadth / 2.0;
            Transform move = Transform.Translation(0, offset, 0);
            sideCurve.Transform(move);
             

            // create extrusion
            double breathExtrude = mittelplate.VesselsBreadth;
            Vector3d vectorToSBSide = new Vector3d(0, breathExtrude, 0);

            Brep SBSideBrep = service.Operations3D.BrepCreator.CreateExtrudedBrep(sideCurve, vectorToSBSide);
            if (SBSideBrep == null)
            {
                RhinoApp.WriteLine("Error: extrusion creation failed. = > drawVesselBaseFromSideView");
                return null;
            }

            return SBSideBrep; 
        }

        private Brep drawVesselBaseFromTopView(RhinoDoc doc, string pathFileName)
        {
            Curve topCurve = drawVesselsCurve("topCurve", pathFileName);   


            // create extrusion
            double  extrude = mittelplate.VesselsHeight;
            Vector3d vector  = new Vector3d(0, 0, extrude * 2);

            Brep topBrep = service.Operations3D.BrepCreator.CreateExtrudedBrep(topCurve, vector);
            if (topBrep == null)
            {
                RhinoApp.WriteLine("Error: extrusion creation failed. = > drawVesselBaseFromTopView");
                return null;
            }

            return topBrep;
        }

    }
}


