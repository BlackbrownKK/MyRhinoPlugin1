using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino;
using System;
using System.Collections.Generic;
using MyRhinoPlugin1.models;

namespace MyRhinoPlugin1.commands
{
    public class AddingTestCargo : Command
    {
        public AddingTestCargo()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        public static AddingTestCargo Instance { get; private set; }



        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "AddingTestCargo";


        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {


            // create rendom cargo <CargoModel>
            List<CargoModel> cargoList = new List<CargoModel>();
            for (int i = 0; i < 3; i++)
            {
                cargoList.Add(new CargoModel()
                {
                    Weight = 8000,
                    Length = 5300,
                    Width = 3500,
                    Height = 3000,
                    Quentity = 1,
                    Name = "Cargo" + i
                });
            }




            
           data.DataModelHolder.Instance.CargoList = cargoList;



            List<Brep> cargoCollection = new List<Brep>();

            //find the vesselConstruction item in the drawing end get it base point
            Point3d basePoint = new Point3d(0, 0, 0);

            foreach (var item in doc.Objects)
            {
             if (item.Name == "vesselConstruction")  

                {
                    if (item.Geometry is Brep brep)
                    {
                        BoundingBox bbox = brep.GetBoundingBox(true);
                        basePoint = bbox.Min;
                        break;
                    }
                }
            }
            // Offset the base point in Y direction
            basePoint = new Point3d(basePoint.X, basePoint.Y + -5000, basePoint.Z); 


            foreach (var cargo in cargoList)
            {
                for (int i = 0; i < cargo.Quentity; i++)
                {
                    RhinoApp.WriteLine($"Cargo {cargo.ToString()}");

                    // Define the box corner points
                    Point3d corner1 = basePoint;
                    Point3d corner2 = new Point3d(corner1.X + cargo.Length, corner1.Y + cargo.Width, corner1.Z + cargo.Height);

                    // Create a box using a bounding rectangle
                    BoundingBox bbox = new BoundingBox(corner1, corner2);
                    Box box = new Box(bbox);

                    // Move the origin for the next box (if needed)
                    basePoint.X += cargo.Length + 1000; // Offset boxes in X direction
                    // make a rendom color for the box
      
               
                   
                 

                    // Create and add the Brep representation of the box to the document
                    cargoCollection.Add(box.ToBrep());
                }
            }





            // Create a new layer for the cargo
            string layerName = "CargoLayer";
            Layer cargoLayer = doc.Layers.FindName(layerName);
            if (cargoLayer == null)
            {
                cargoLayer = new Layer
                {
                    Name = layerName,
                    IsVisible = true,
                    IsLocked = false
                };
                doc.Layers.Add(cargoLayer);
            }
            // Add the cargo Breps to the document and assign them to the new layer

     
            foreach (Brep cargoBrep in cargoCollection)
            {
                // Add the Brep to the document and get the Guid of the object
                Guid objGuid = doc.Objects.AddBrep(cargoBrep);
                // Get the RhinoObject associated with the Guid
                RhinoObject obj = doc.Objects.Find(objGuid);
                if (obj != null)
                {
                    Layer cargoLayerTemp = doc.Layers.FindName("CargoLayer");
                    // Assign the Brep to the "CargoLayer" layer
                    obj.Attributes.LayerIndex = cargoLayerTemp.Index;
                    obj.Attributes.Name = "CargoUnit";

                    // Create a random color
                    Random random = new Random();
                    int r = random.Next(0, 256);
                    int g = random.Next(0, 256);
                    int b = random.Next(0, 256);
                    System.Drawing.Color randomColor = System.Drawing.Color.FromArgb(r, g, b);
                    // Set the color of the box
                    obj.Attributes.ObjectColor = randomColor;
                    obj.Attributes.ColorSource = ObjectColorSource.ColorFromObject;
                    // Commit changes to the object
                    obj.CommitChanges();
                }
            }

            doc.Views.Redraw();
            RhinoApp.WriteLine("Packing list imported successfully.");
            RhinoDoc.ActiveDoc.Views.Redraw();
            return Result.Success;
        }
    }
}

