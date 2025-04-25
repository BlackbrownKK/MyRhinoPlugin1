using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRhinoPlugin1.service.Operations3D
{
    public static class BrepCreator
    {
        /// <summary>
        /// Extrudes a curve along a given vector and returns a Brep.
        /// </summary>
        /// <param name="curve">The input curve to extrude.</param>
        /// <param name="direction">The extrusion direction vector.</param>
        /// <returns>A Brep resulting from the extrusion, or null if failed.</returns>
        public static Brep CreateExtrudedBrep(Curve curve, Vector3d direction)
        {
            if (curve == null || !curve.IsValid || direction.IsZero)
                return null;

            // Create the extrusion
            Surface surface = Surface.CreateExtrusion(curve, direction);
            if (surface == null)
                return null;

            // Convert surface to Brep
            Brep brep = surface.ToBrep();

            // Cap the planar holes to make it a solid
            if (brep != null && !brep.IsSolid)
            {
                brep = brep.CapPlanarHoles(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            }
            return brep;

        }
    }
}
