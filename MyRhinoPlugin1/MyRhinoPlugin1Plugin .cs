using Rhino.PlugIns;
using Rhino.UI;
using MyRhinoPlugin1.userInterface;
using Eto.Drawing;  // Use Eto for cross-platform UI
using System.Drawing;
using MyRhinoPlugin1.commands;
using Rhino;
namespace MyRhinoPlugin1
{
    ///<summary>
    /// <para>Every RhinoCommon .rhp assembly must have one and only one PlugIn-derived
    /// class. DO NOT create instances of this class yourself. It is the
    /// responsibility of Rhino to create an instance of this class.</para>
    /// <para>To complete plug-in information, please also see all PlugInDescription
    /// attributes in AssemblyInfo.cs (you might need to click "Project" ->
    /// "Show All Files" to see it in the "Solution Explorer" window).</para>
    ///</summary>
    public class MyRhinoPlugin1Plugin : PlugIn
    {
        public MyRhinoPlugin1Plugin()
        {
            Instance = this;
        }

        // todo cross section ! aft + side 

        public static MyRhinoPlugin1Plugin Instance { get; private set; }

        private static CrossSectionMouseListener _mouseListener;
        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            _mouseListener = new CrossSectionMouseListener();
            _mouseListener.Enabled = true;
            behavior.CollisionGuard.Enable();
            return LoadReturnCode.Success;
        }

        // New method to trigger the cross-section listener or handle actions

        
    }
} 