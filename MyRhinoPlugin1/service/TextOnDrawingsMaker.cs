using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRhinoPlugin1.service
{
    public static class TextOnDrawingsMaker
    {
        public static void AddLayoutText(RhinoDoc doc, string text, Point3d location, double height, string layerName)
        {
            var textEntity = new TextEntity
            {
                PlainText = text,
                Plane = Plane.WorldXY,
                TextHeight = height,
                Justification = TextJustification.Left,
                TextHorizontalAlignment = TextHorizontalAlignment.Left,
                TextVerticalAlignment = TextVerticalAlignment.Bottom,
            };

            var plane = Plane.WorldXY;
            plane.Origin = location;
            textEntity.Plane = plane;

            int layerIndex = doc.Layers.FindByFullPath(layerName, -1);
            if (layerIndex == -1)
                layerIndex = doc.Layers.Add(layerName, System.Drawing.Color.Black);

            var attr = new ObjectAttributes
            {
                LayerIndex = layerIndex,
                Space = ActiveSpace.PageSpace
            };

            doc.Objects.AddText(textEntity, attr);
        }
    }
}
