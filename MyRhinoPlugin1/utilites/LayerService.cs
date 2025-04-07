using Rhino.DocObjects;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRhinoPlugin1.utilites
{
    public static class LayerService
    {
        // Method to get or create the "vesselConstruction" layer
        public static Layer GetOrCreateLayer(RhinoDoc doc, string layerName)
        {
            // Search for the existing layer by name
            Layer layer = doc.Layers.FindName(layerName);

            if (layer == null)
            {
                // If the layer doesn't exist, create a new one
                layer = new Layer { Name = layerName };
                doc.Layers.Add(layer);
            }

            return layer;
        } 
    }
}
