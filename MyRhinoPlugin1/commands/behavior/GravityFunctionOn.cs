using MyRhinoPlugin1.behavior.gravity;
using Rhino;
using Rhino.Commands;

namespace MyRhinoPlugin1.commands.behavior
{
    public class GravityFunctionOn : Command
    {
        public override string EnglishName => "GravityFunctionOn";

        public static GravityFunctionOn Instance { get; private set; }
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            GravityWatcher.Enable();
            return Result.Success;
        }
    }
}
