using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace MyRhinoPlugin1.commands
{
    public class MoveAllTDs : Command

    {

        public static MoveAllTDs Instance { get; private set; }

     



        public MoveAllTDs()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a reference in a static property.
            Instance = this;
        }

        public override string EnglishName => "MoveAllTDToLowerPosition";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Define the translation vector (e.g., moving 10 units down in the Z direction)
            Vector3d translationVector = new Vector3d(0, 0, -1000);
            // Iterate through all objects in the document
            foreach (RhinoObject obj in doc.Objects)
            {
                // Check if the object is on the "TD" layer
                if (obj.Attributes.LayerIndex == doc.Layers.FindName("TD").Index)
                {
                    // Apply the translation transformation
                    Transform translation = Transform.Translation(translationVector);
                    obj.Geometry.Transform(translation);

                    // Commit the changes to the object
                    obj.CommitChanges();
                }

                
               
            }
            doc.Views.Redraw();
            return Result.Success;
        }
    }
}

