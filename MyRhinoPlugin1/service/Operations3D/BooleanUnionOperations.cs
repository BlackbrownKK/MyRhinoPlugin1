using Rhino.Geometry;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRhinoPlugin1.service.Operations3D
{
    public static class BooleanUnionOperations
    {
        /// <summary>
        /// Performs a Boolean Union operation between two Breps.
        /// </summary>
        public static Brep[] PerformBooleanUnion(List<Brep> allBreps, RhinoDoc doc)
        {
            Brep[] unionResult = Brep.CreateBooleanUnion(allBreps.ToArray(), doc.ModelAbsoluteTolerance);

            // Handle cases where Boolean Union fails
            if (unionResult == null || unionResult.Length == 0)
            {
                RhinoApp.WriteLine("Error: Boolean Union failed.");
                return null;
            }

            return unionResult;
        }
    }
}
