using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.UI;


namespace MyRhinoPlugin1.controllers
{
    public static class OpenFilesWithBlockController
    {


        public static InstanceDefinition OpenFilesWithBlock(RhinoDoc doc, string fileName, string blockName)
        {
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string path = Path.Combine(projectRoot, "vesselsDigitalModels", fileName);

            if (!File.Exists(path))
            {
                RhinoApp.WriteLine($"File not found: {path}");
                return null;
            }

            File3dm externalFile = File3dm.Read(path);
            if (externalFile == null)
            {
                RhinoApp.WriteLine("Failed to read 3dm file.");
                return null;
            }

            var fileBlock = externalFile.InstanceDefinitions.FirstOrDefault(def => def.Name == blockName);
            if (fileBlock == null)
            {
                RhinoApp.WriteLine($"Block '{blockName}' not found in file.");
                return null;
            }

            var geometryList = new List<GeometryBase>();
            var attributesList = new List<ObjectAttributes>();

            // Temporary fallback origin
            Point3d blockOrigin = Point3d.Origin;

            bool originSet = false;

            foreach (var id in fileBlock.GetObjectIds())
            {
                var fileObj = externalFile.Objects.FindId(id);
                if (fileObj != null)
                {
                    var geo = fileObj.Geometry.Duplicate();

                    if (!originSet && geo is Point point)
                    {
                        //blockOrigin = point.Location;
                        originSet = true;
                    }

                    var attr = fileObj.Attributes.Duplicate();

                 

                    geometryList.Add(geo);
                    attributesList.Add(attr);
                }
            }

            if (geometryList.Count == 0)
            {
                RhinoApp.WriteLine("No geometry found in block.");
                return null;
            }

            int newDefIndex = doc.InstanceDefinitions.Add(blockName, fileBlock.Description, blockOrigin, geometryList, attributesList);
            if (newDefIndex < 0)
            {
                RhinoApp.WriteLine("Failed to add block definition.");
                return null;
            }

            InstanceDefinition definition = doc.InstanceDefinitions[newDefIndex];
            RhinoApp.WriteLine($"Loaded block: {blockName} + X: {blockOrigin.X} + Y:{blockOrigin.Y} + Z:{blockOrigin.Z}  ");
            // Return the block definition
            return definition;
        }
    }
}





