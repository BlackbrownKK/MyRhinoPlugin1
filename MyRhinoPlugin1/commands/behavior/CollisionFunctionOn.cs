using Rhino.Commands;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyRhinoPlugin1.behavior;
using MyRhinoPlugin1.behavior.collision;

namespace MyRhinoPlugin1.commands.behavior
{
    public class CollisionFunctionOn : Rhino.Commands.Command
    {
        public override string EnglishName => "CollisionFunctionOn";

        public static CollisionFunctionOn Instance { get; private set; }
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            CollisionGuard.Enable();
            return Result.Success;
        }
    }
}
