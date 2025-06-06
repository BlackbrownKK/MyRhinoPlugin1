using MyRhinoPlugin1.userInterface;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRhinoPlugin1.commands.layout
{
    public class EditTestAttributes: Command
    {

        private static TextLayoutEditor _formInstance;


        public EditTestAttributes()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        public static EditTestAttributes Instance { get; private set; }



        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "EditTestAttributes";


        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
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

            return Result.Success;
        }
    }
}
 