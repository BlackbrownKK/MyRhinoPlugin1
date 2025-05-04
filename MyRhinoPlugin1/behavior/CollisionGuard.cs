using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRhinoPlugin1.behavior
{
    public class CollisionGuard
    {
        private static bool _enabled = false;
        private static Dictionary<Guid, GeometryBase> _originalGeometries = new Dictionary<Guid, GeometryBase>();
        public static void Enable()
        {
            if (_enabled)
                return;

            RhinoDoc.ReplaceRhinoObject += OnReplaceRhinoObject;
            _enabled = true;
            RhinoApp.WriteLine("CollisionGuard enabled.");
        }

        public static void Disable()
        {
            if (!_enabled)
                return;

            RhinoDoc.ReplaceRhinoObject -= OnReplaceRhinoObject;
            _enabled = false;
            RhinoApp.WriteLine("CollisionGuard disabled.");
        }

        private static void OnReplaceRhinoObject(object sender, RhinoReplaceObjectEventArgs e)
        {
            var doc = RhinoDoc.ActiveDoc;
            var newObj = e.NewRhinoObject;
            var oldObj = e.OldRhinoObject;

            if (newObj == null || !(newObj.Geometry is Brep) || oldObj == null || !(oldObj.Geometry is Brep))
                return;

            // Compute movement vector (approximate)
            var newBrep = (Brep)newObj.Geometry;
            var oldBrep = (Brep)oldObj.Geometry;

            var oldCenter = oldBrep.GetBoundingBox(true).Center;
            var newCenter = newBrep.GetBoundingBox(true).Center;
            var moveVec = newCenter - oldCenter;

            if (moveVec.IsZero)
                return;

            // Check collision with all other Breps
            var otherBreps = doc.Objects
                .Where(o => o.Id != newObj.Id && o.Geometry is Brep)
                .Select(o => o.Geometry as Brep)
                .ToList();

            foreach (var other in otherBreps)
            {
                var intersect = Brep.CreateBooleanIntersection(newBrep, other, doc.ModelAbsoluteTolerance);
                if (intersect != null && intersect.Count() > 0)
                {
                    RhinoApp.WriteLine($"Collision detected. Moving object back...");

                    // Step back along negative move vector
                    const int maxSteps = 20;
                    Vector3d step = -moveVec / maxSteps;
                    Brep movedBack = newBrep.DuplicateBrep();

                    for (int i = 0; i < maxSteps; i++)
                    {
                        var xform = Transform.Translation(step);
                        movedBack.Transform(xform);

                        var testIntersect = Brep.CreateBooleanIntersection(movedBack, other, doc.ModelAbsoluteTolerance);
                        if (testIntersect == null || testIntersect.Count() == 0)
                        {
                            doc.Objects.Replace(newObj.Id, movedBack);
                            doc.Views.Redraw();
                            RhinoApp.WriteLine("Object moved back to avoid collision.");
                            return;
                        }
                    }

                    RhinoApp.WriteLine("Unable to resolve collision with step-back.");
                    return;
                }
            }
        }
    }
}