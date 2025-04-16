 
using Rhino.Commands;

namespace MyRhinoPlugin1.commands
{
    public class Start : Command
    {
        public static Start Instance { get; private set; }


        public override string EnglishName => "start";

        protected override Result RunCommand(Rhino.RhinoDoc doc, RunMode mode)
        {
            // Show the Eto form
            var form = new userInterface.MyEtoForm();
            form.Show(); // or ShowModal if you want blocking behavior
            return Result.Success;
        }

        
    }
}
