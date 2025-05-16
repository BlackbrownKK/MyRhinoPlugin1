using Rhino.PlugIns;  
namespace MyRhinoPlugin1
{
 
    public class MyRhinoPlugin1Plugin : PlugIn
    {
        public MyRhinoPlugin1Plugin()
        {
            Instance = this;
        } 
        public static MyRhinoPlugin1Plugin Instance { get; private set; }

      
        protected override LoadReturnCode OnLoad(ref string errorMessage)
        { 
            return LoadReturnCode.Success;

        } 
    }
} 