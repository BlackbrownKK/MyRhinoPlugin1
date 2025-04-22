using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;
using Eto.Drawing;
using Rhino;
using Rhino.UI;
using System;
using System.Runtime.InteropServices; // Add this!
                                      // Make sure it's inside 'UI'

namespace MyRhinoPlugin1.userInterface
 
{
    [Guid("12345678-ABCD-4321-ABCD-123456789ABC")] // Use a uni
    public class BuildModelPanel : Panel
    {
        public BuildModelPanel()
        {
            // Create a button
            var buildButton = new Button { Text = "Build Vessel Model" };

            // Add event to execute the Rhino command
            buildButton.Click += (sender, e) =>
            {
              RhinoApp.RunScript("_BuildModel", false);
            };

            // Layout
            Content = new StackLayout
            {
                Padding = 10,
                Items = { buildButton }
            };
        }
    }
}


