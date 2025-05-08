using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace MyRhinoPlugin1.commands
{
    public class Start : Command
    {
        public static Start Instance { get; private set; }
        private static userInterface.MyEtoForm _formInstance;

        public override string EnglishName => "start";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // ✅ Prevent multiple windows
            if (_formInstance == null || !_formInstance.Visible)
            {
                _formInstance = new userInterface.MyEtoForm();
                _formInstance.Closed += (s, e) => _formInstance = null; // Clear ref on close
                _formInstance.Show(); // Use ShowModal() if blocking is needed


/* Unmerged change from project 'MyRhinoPlugin1 (net7.0)'
Before:
                service.GravityWatcher.Enable();
                userInterface.CustomRhinoToolsBarInterface.CustomAllPannels();
After:
                GravityWatcher.Enable();
                userInterface.CustomRhinoToolsBarInterface.CustomAllPannels();
*/
                behavior.gravity.GravityWatcher.Enable();
                userInterface.CustomRhinoToolsBarInterface.CustomAllPannels();
            }
            else
            {
                RhinoApp.WriteLine("The UI is already open.");
            }

            return Result.Success;
        }

    }
}
