using System;
using System.Collections.Generic;
using System.Diagnostics;
using Eto.Forms;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;



namespace MyRhinoPlugin1.commands
{
    public class ImportPackingList : Rhino.Commands.Command
    {
        public ImportPackingList()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        public static ImportPackingList Instance { get; private set; }


        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "ImportPackingList";


        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Open file dialog for CSV selection

            var openFileDialog = new Rhino.UI.OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Select Packing List Excel File"
            };

            // Show the dialog and check if a file was selected
            if (openFileDialog.ShowOpenDialog() == false)
            {
                RhinoApp.WriteLine("No file selected.");
                return Result.Cancel;
            }

            string filePath = openFileDialog.FileName;
            ImportController importController = new ImportController(filePath);

            List<models.CargoModel> cargoList = importController.CargoList;
            // print debug writeline this cargoList

            List<Brep> cargoCollection = new List<Brep>();

            Point3d origin = new Point3d(0, 0, 0);
            foreach (var cargo in cargoList)
            {
                for (int i = 0; i < cargo.Quentity; i++)
                {
                    RhinoApp.WriteLine($"Cargo {cargo.ToString()}");

                    // Define the box corner points
                    Point3d corner1 = origin;
                    Point3d corner2 = new Point3d(corner1.X + cargo.Length, corner1.Y + cargo.Width, corner1.Z + cargo.Height);

                    // Create a box using a bounding rectangle
                    BoundingBox bbox = new BoundingBox(corner1, corner2);
                    Box box = new Box(bbox);

                    // Move the origin for the next box (if needed)
                    origin.X += cargo.Length + 100; // Offset boxes in X direction

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

            RhinoApp.RunScript("_New", false);
            foreach (Brep cargoBrep in cargoCollection)
            {
                RhinoDoc.ActiveDoc.Objects.Add(cargoBrep);
                RhinoDoc.ActiveDoc.Views.Redraw();
            }
            foreach (Brep cargoBrep in cargoCollection)
            {
                // Add the Brep to the document and get the Guid of the object
                System.Guid objGuid = doc.Objects.AddBrep(cargoBrep);
                // Get the RhinoObject associated with the Guid
                RhinoObject obj = doc.Objects.Find(objGuid);
                if (obj != null)
                {
                    // Assign the Brep to the "CargoLayer" layer
                    obj.Attributes.LayerIndex = cargoLayer.Index;
                    // Commit changes to the object
                    obj.CommitChanges();
                }
            }

            doc.Views.Redraw();
            RhinoApp.WriteLine("Packing list imported successfully.");
            return Result.Success;
        }
    }
}


