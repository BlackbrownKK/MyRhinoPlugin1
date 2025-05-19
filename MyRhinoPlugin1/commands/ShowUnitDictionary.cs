using Rhino.Commands;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.DocObjects;

namespace MyRhinoPlugin1.commands
{
    public class ShowUnitDictionary : Command
    {
        public static ShowUnitDictionary Instance { get; private set; }



        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "ShowUnitDictionary";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var objRef = Rhino.Input.RhinoGet.GetOneObject("Select object", false, ObjectType.AnyObject, out var rhinoObjRef);
            if (rhinoObjRef != null)
            {
                var userDict = rhinoObjRef.Object().Attributes.UserDictionary;
                foreach (var key in userDict.Keys)
                {
                    RhinoApp.WriteLine($"{key}: {userDict[key]}");
                }
            }
            return Result.Success;
        }
    }
}
