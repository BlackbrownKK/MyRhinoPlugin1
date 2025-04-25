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

         
        public override string EnglishName => "start";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Show the Eto form
            var form = new userInterface.MyEtoForm();
            form.Show(); // or ShowModal if you want blocking behavior
            service.GravityWatcher.Enable();
            return Result.Success;
        } 
    }
}