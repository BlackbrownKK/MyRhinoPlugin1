using Rhino.Geometry;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRhinoPlugin1.service.Operations3D
{
    public static class BooleanDifferenceOperations
    {
        /// <summary>
        /// Performs a Boolean Difference operation between two Breps.
        /// </summary>
        /// 
        public static Brep[] PerformBooleanDifference(Brep baseBrep, Brep subtractBrep, RhinoDoc doc)
        {
            if (baseBrep == null || subtractBrep == null)
                return null;

            Brep[] result = Brep.CreateBooleanDifference(baseBrep, subtractBrep, doc.ModelAbsoluteTolerance);

            return result;
        }
    }
}
