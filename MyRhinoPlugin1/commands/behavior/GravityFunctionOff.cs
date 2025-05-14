using System;
using System.Collections.Generic;
using System.Diagnostics;
using Eto.Forms;
using MyRhinoPlugin1.controllers.importController;
using MyRhinoPlugin1.data;
using MyRhinoPlugin1.models;
using MyRhinoPlugin1.behavior.gravity;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;

 

namespace MyRhinoPlugin1.commands.behavior
{
    public class GravityFunctionOff : Rhino.Commands.Command
    {
        public override string EnglishName => "GravityFunctionOff";

        public static GravityFunctionOff Instance { get; private set; }
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            GravityWatcher.Disable();
            return Result.Success;
        }
    }
}
