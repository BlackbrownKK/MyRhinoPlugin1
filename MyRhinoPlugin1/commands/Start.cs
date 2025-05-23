﻿using Rhino;
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
                 
            }
            else
            {
                RhinoApp.WriteLine("The UI is already open.");
            }

            return Result.Success;
        }

    }
}
