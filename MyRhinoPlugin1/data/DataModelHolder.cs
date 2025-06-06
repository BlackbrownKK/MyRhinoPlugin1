using MyRhinoPlugin1.models;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MyRhinoPlugin1.data
{
    public class DataModelHolder
    {
        
        private static DataModelHolder _instance;
        private VesselModel vessel;
        private List<CargoModel> cargoList;
        public static string CompamnyName { get; set; }
        public static string VesselName { get; set; }
        public static string VesselType { get; set; }

        public static string titel { get; set; }
        public static string subject { get; set; }

        public static string rev { get; set; }
        public static string planner { get; set; }
        public static string planType { get; set; }
        public static string planStatus { get; set; }
        public static string jobId { get; set; } 
        public static string Date { get; set; }

 


        // Singleton pattern to ensure only one instance of DataModelHolder exists
        public static DataModelHolder Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DataModelHolder();
                }
                return _instance;
            }
        }

        private DataModelHolder()
        {
            CompamnyName = "BRIESE Chartering";
            VesselName = "VesselName";
            VesselType = "VesselType";
            titel = "titel";
            subject = "subject";
            rev = "REV";
            planner = "planner";
            planType = "planType";
            planStatus = "planStatus";
            jobId = "lobId";
            Date = DateTime.Now.ToString("ddMMMM yyyy").ToUpper().Replace(" ", "");
        }

        // Store a single vessel
        public VesselModel Vessel { get => vessel; set => vessel = value; }

        // Store a single cargo list.
        public List<CargoModel> CargoList
        {
            get => cargoList;
            set
            {
                cargoList = value;
                // You can add additional logic here if needed when setting the cargo list
            }
        }




    }
}
