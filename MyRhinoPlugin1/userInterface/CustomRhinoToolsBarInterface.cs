using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.UI;

namespace MyRhinoPlugin1.userInterface
{
    public class CustomRhinoToolsBarInterface
    {

        public static void CustomAllPannels()
        {
            CloseAllToolbars();
            plaginToolBarButtonCreator();
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
        private static void plaginToolBarButtonCreator()
        {
            
        }
    }
} 