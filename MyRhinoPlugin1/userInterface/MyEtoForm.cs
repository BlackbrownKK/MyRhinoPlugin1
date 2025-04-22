using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;
using Eto.Drawing;
using Rhino;
using Rhino.Geometry;
using Point = Rhino.Geometry.Point;

namespace MyRhinoPlugin1.userInterface
{
    public class MyEtoForm : Form
    {
        private int _boxX = 10; // Initial X position of the box
        private int _boxY = 10; // Initial Y position of the box
        private bool _isDragging = false; // Track whether the box is being dragged
        private PointF _startPoint; // ✅ Eto.Drawing.Point
        private PointF _startPoint2; // Eto.Drawing.PointF

        public MyEtoForm()
        {
            Title = "My Eto Form";
            ClientSize = new Size(600, 500);

            var drawable = new Drawable
            {
                Size = new Size(400, 300),
                BackgroundColor = Colors.White
            };

            // Handle the mouse down event to start dragging
            drawable.MouseDown += (sender, e) =>
            {
                var mousePos = e.Location;

                // Check if the click is inside the box
                if (mousePos.X >= _boxX && mousePos.X <= _boxX + 100 &&
                    mousePos.Y >= _boxY && mousePos.Y <= _boxY + 50)
                {
                    _isDragging = true; // Start dragging
                    _startPoint = mousePos; // Set the starting point of the drag
                }
            };

            // Handle the mouse move event to move the box
            drawable.MouseMove += (sender, e) =>
            {
                if (_isDragging)
                {
                    var mousePos = e.Location;
                    // Calculate the offset the mouse has moved
                    var offsetX = mousePos.X - _startPoint.X;
                    var offsetY = mousePos.Y - _startPoint.Y;

                    // Update the box position based on the offset
                    _boxX += (int)offsetX;
                    _boxY += (int)offsetY;

                    // Update the start point for the next movement
                    _startPoint = mousePos;

                    // Redraw the drawable to show the box in the new position
                    drawable.Invalidate();
                }
            };

            // Handle the mouse up event to stop dragging
            drawable.MouseUp += (sender, e) =>
            {
                _isDragging = false; // Stop dragging
            };

            // Handle the drawing of the box
            drawable.Paint += (sender, pe) =>
            {
                var g = pe.Graphics;
                g.Clear(Colors.White);

                // Draw the box at the new position
                g.DrawRectangle(Colors.Black, _boxX, _boxY, 100, 50);
            };

            var label = new Label { Text = "Hello Eto.Forms!" };

            var buildModelButton = new Button { Text = "Build the Mittelplate model" };
            buildModelButton.Click += (sender, e) =>
            {
                Rhino.RhinoApp.RunScript("BuildModel", false);
            };

            var importPackingListButton = new Button { Text = "Import Packing List" };
            importPackingListButton.Click += (sender, e) =>
            {
                Rhino.RhinoApp.RunScript("ImportPackingList", false);
            };


            var moveTDButton = new Button { Text = "Move All TDs Down" };
            moveTDButton.Click += (sender, e) =>
            {
                Rhino.RhinoApp.RunScript("MoveAllTDToLowerPosition", false);
            };


            // Layout
            var layout = new DynamicLayout { Padding = 10, Spacing = new Size(5, 5) };
            layout.Add(drawable);
            layout.Add(label);
            layout.AddSeparateRow(moveTDButton);
            layout.AddSeparateRow(buildModelButton, importPackingListButton);

            Content = layout;
        }
    }
}

 