using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Rhino.UI;

namespace MyRhinoPlugin1.service
{
    public static class GravityWatcher
    {
        private static bool _enabled = false;
        //Stores the ID of the most recently moved object (captured in ReplaceRhinoObject event), to process during idle time.
        private static Guid _lastMovedObjectId = Guid.Empty;
        private static HashSet<Guid> _alreadyProcessed = new HashSet<Guid>();

        //Registers your gravity watcher when you want to start listening to object movements.
        public static void Enable()
        {
            if (_enabled)
                return;

     


            //Avoids double-adding event handlers.
            RhinoDoc.ReplaceRhinoObject += OnObjectTransformed;
            RhinoApp.Idle += OnIdle;
            _enabled = true;

            RhinoApp.WriteLine("GravityWatcher enabled.");
        }


        //Cleans up the event handlers and deactivates the watcher.
        public static void Disable()
        {
            if (!_enabled)
                return;

            RhinoDoc.ReplaceRhinoObject -= OnObjectTransformed;
            RhinoApp.Idle -= OnIdle;
            _enabled = false;

            RhinoApp.WriteLine("GravityWatcher disabled.");
        }
        //When an object is moved, this event fires. Instead of processing right away (which can cause issues),
        //it just stores the object's ID for later processing when Rhino is idle.

        private static void ClearProcessed(object sender, EventArgs e)
        {
            _alreadyProcessed.Clear();
            RhinoApp.Idle -= ClearProcessed;
        }



        private static void OnObjectTransformed(object sender, RhinoReplaceObjectEventArgs e)
        {
            // Save the ID to handle it during idle (when Rhino is idle and safe to transform)
            _lastMovedObjectId = e.NewRhinoObject.Id;
        }


        //This is the heart of the gravity system — it gets called when Rhino isn't busy (idle event).
        private static void OnIdle(object sender, EventArgs e)
        {
            if (_lastMovedObjectId == Guid.Empty)
                return;

            var doc = RhinoDoc.ActiveDoc;

            // Skip gravity if active viewport is Top
            var activeViewport = doc.Views.ActiveView.ActiveViewport;
            if (activeViewport.IsParallelProjection && activeViewport.Name == "Top View")
                return;


            var obj = doc.Objects.FindId(_lastMovedObjectId);

            // Clear the ID immediately to avoid reentry
            var currentId = _lastMovedObjectId;
            _lastMovedObjectId = Guid.Empty;

            if (obj == null || !(obj.Geometry is Brep brep))
                return;

            // Prevent reprocessing the same object
            if (_alreadyProcessed.Contains(currentId))
                return;

            _alreadyProcessed.Add(currentId);

            // Clear processed list on next idle
            RhinoApp.Idle -= ClearProcessed;
            RhinoApp.Idle += ClearProcessed;

            // Collect ground
            var groundBreps = doc.Objects
                .Where(o => o.Id != obj.Id && o.Geometry is Brep)
                .Select(o => o.Geometry as Brep)
                .ToList();

            // Apply gravity
            if (GravityFunction.TrySnapToGround(brep, groundBreps, out Transform moveDown))
            {
                doc.Objects.Transform(obj.Id, moveDown, true);
                RhinoApp.WriteLine($"Gravity applied to {obj.Id}");
                doc.Views.Redraw();
            }
            else
            {
                RhinoApp.WriteLine($"No ground found under {obj.Id}.");
            }
        }
    }
}