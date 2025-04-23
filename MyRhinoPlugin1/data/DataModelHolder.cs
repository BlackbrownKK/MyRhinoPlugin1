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
            // Initialize data if needed
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
