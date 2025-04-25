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


            Curve test = drawVesselSideCurve();
            doc.Objects.AddCurve(test);

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
            
            foreach(var tD in TDList)
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
            Brep[] vesselBodyBrepArray = drawVesselBase();
            Brep vesselBodyBrep = vesselBodyBrepArray[0]; // Assuming you want the first Brep from the array

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




        private Brep drawVesselsHold()
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

            return new Box(new BoundingBox(minPoint, maxPoint)).ToBrep();

        }



        // Method to draw the vessel body. Here creare
        private Curve drawVesselSideCurve()
        {
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string path = Path.Combine(projectRoot, "vesselsDigitalModels", "SideCurveMittelplateBody.txt");
            RhinoApp.WriteLine(path);
            // Check if the file exists
            if (!File.Exists(path))
        {
        RhinoApp.WriteLine($"Error: File not found at {path}");
        return null;

        }
            List <Point3d> sideCurvE = service.ReadPointsFromTxt.PointsReader(path);
            service.CerveCreator curveCreator = new service.CerveCreator("SideCurve", sideCurvE);
            Curve curve = curveCreator.CerveCreatorByListOfPoints();

            // Make sure curve is valid and closed
            if (curve == null || !curve.IsClosed)
            {
                RhinoApp.WriteLine("Curve is null or not closed.");
                return null;
            }
            return curve;
        }

        private Brep[] drawVesselBase()
        {
            Curve curve = drawVesselSideCurve();
            RhinoApp.WriteLine(curve.ToString());
            double breath = mittelplate.VesselsBreadth;

            Vector3d vectorToSBSide = new Vector3d(0, -breath, 0);
            Vector3d vectorToPSSide = new Vector3d(0, breath, 0);

            // create a brep from the curve
            Brep SBSideBrep = service.Operations3D.BrepCreator.CreateExtrudedBrep(curve, vectorToSBSide);
            // Check if the extrusion was successful
            if (SBSideBrep == null)
            {
                RhinoApp.WriteLine("Error:  drawVesselBase() =>  SBSideBrep is null.");
                return null;
            }
            Brep PSSideBrep = service.Operations3D.BrepCreator.CreateExtrudedBrep(curve, vectorToPSSide);
            // Check if the extrusion was successful
            if (PSSideBrep == null)
            {
                RhinoApp.WriteLine("Error:  drawVesselBase() =>  PSSideBrep is null.");
                return null;
            }

            // join the two sides
            RhinoApp.WriteLine($"SBSideBrep IsSolid: {SBSideBrep.IsSolid}");
            RhinoApp.WriteLine($"PSSideBrep IsSolid: {PSSideBrep.IsSolid}");
            Brep[] joinedBreps = service.Operations3D.BooleanUnionOperations.PerformBooleanUnion(new List<Brep> { SBSideBrep, PSSideBrep }, RhinoDoc.ActiveDoc);
            // Check if the join was successful
            if (joinedBreps == null || joinedBreps.Length == 0)
            {
                RhinoApp.WriteLine("Error:  drawVesselBase() =>  Boolean union failed.");
                return null;
            }
            return joinedBreps;
        }
    }
}


