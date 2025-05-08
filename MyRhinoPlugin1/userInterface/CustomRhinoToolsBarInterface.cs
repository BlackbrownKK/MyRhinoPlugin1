using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.UI;
using MyRhinoPlugin1.commands;

namespace MyRhinoPlugin1.userInterface
{
    public class CustomRhinoToolsBarInterface
    {

        public static void CustomAllPannels(RhinoDoc doc)
        {
            CloseAllToolbars();
            CustomViewportLayoutCommand.custom3ViewsMaker(doc);
        }



        private static void CloseAllToolbars()
        {
            try
            {
                foreach (var f in RhinoApp.ToolbarFiles)
                {
                    f.Close(false);
                }
            }
            catch { }

            foreach (var p in Panels.GetOpenPanelIds())
            {
                Panels.ClosePanel(p);
            }

            ToolbarFileCollection.SidebarIsVisible = false;
        }
     
    }
} 