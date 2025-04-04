using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Commands;
using Rhino.UI; 

namespace MyRhinoPlugin1.userInterface
{
    public class ShowBuildModelPanel : Command
    {
        public override string EnglishName => "ShowBuildModelPanel";
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Show the UI panel
            Panels.OpenPanel(typeof(BuildModelPanel).GUID);
            return Result.Success;
        }
    }
}
