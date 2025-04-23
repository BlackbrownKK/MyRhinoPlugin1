using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Rhino;
using Rhino.Geometry;
using Point = Rhino.Geometry.Point;

namespace MyRhinoPlugin1.userInterface
{
    public class MyEtoForm : Form
    {
        private bool _isDragging = false; // Track whether the box is being dragged
        private PointF _startPoint; // ✅ Eto.Drawing.Point
        private float _scale = 1.0f; // Initial scale
        private const float MovementCoefficient = 1.0f; // Adjust the movement sensitivity
        private const float ZoomFactor = 1.1f; // How much to zoom per wheel notch

        private List<CargoVisual> _visualCargo = new List<CargoVisual>();
        private CargoVisual _selectedCargo = null;

        public MyEtoForm()
        {
          
            Title = "My Eto Form";
            ClientSize = new Size(800, 600);

            var drawable = new Drawable
            {
                Size = new Size(500, 400),
                BackgroundColor = Colors.White
            };

            LoadCargoVisuals();



            drawable.Paint += (sender, pe) =>
            {
                var g = pe.Graphics;
                g.Clear(Colors.White);
                g.ScaleTransform(_scale); // Apply zoom to the graphics context


               

                if (_visualCargo.Count == 0)
                {
                    g.DrawText(SystemFonts.Default(), Colors.Red, 10, 10, "No cargo data loaded.");
                }

                // Draw each cargo entity
                foreach (var vis in _visualCargo)
                {
                    g.FillRectangle(vis.FillColor, vis.Bounds);
                    g.DrawRectangle(Colors.Black, vis.Bounds);
                    g.DrawText(SystemFonts.Default(), Colors.Black, vis.Bounds.X + 3, vis.Bounds.Y + 3, vis.Name);
                    RhinoApp.WriteLine(vis.ToString());
                }
            };


            // Handle the mouse down event to start dragging
            drawable.MouseDown += (sender, e) =>
            {
                var mousePos = new PointF(e.Location.X / _scale, e.Location.Y / _scale);
                RhinoApp.WriteLine($"Mouse Down at: {mousePos}");


                // Check if the click is inside any of the cargo boxes
                foreach (var cargo in _visualCargo)
                {
                    if (cargo.Bounds.Contains(mousePos))
                    {
                        _selectedCargo = cargo;  // Set the selected cargo
                        _startPoint = mousePos;  // Set the starting point for dragging
                        RhinoApp.WriteLine($"Mouse Down at: {cargo.Name}");

                        break; // Stop the loop after the first match
                    }
                }
            };


            // Handle the mouse move event to move the box
            drawable.MouseMove += (sender, e) =>
            {
                if (_selectedCargo != null)
                {
                    var currentPos = new PointF(e.Location.X / _scale, e.Location.Y / _scale); 
                    RhinoApp.WriteLine($"Mouse Move at: {currentPos}"); 
                    var offset = currentPos - _startPoint;
                    RhinoApp.WriteLine($"Offset: {offset}" + $"= {currentPos}" + $"- {_startPoint}");


                    _selectedCargo.Bounds = new RectangleF(
                        _selectedCargo.Bounds.X + offset.X,
                        _selectedCargo.Bounds.Y + offset.Y,
                        _selectedCargo.Bounds.Width,
                        _selectedCargo.Bounds.Height
                    );

                    _startPoint = currentPos; // ✅ Update this line to allow continuous movement
                    drawable.Invalidate();
                }
            };

            // Handle the mouse up event to stop dragging
            drawable.MouseUp += (sender, e) =>
            {
                _selectedCargo = null; // Deselect the cargo when the mouse is released
            };

            // Handle mouse wheel for zooming
            drawable.MouseWheel += (sender, e) =>
            {
                if (e.Delta.Height > 0)
                    _scale *= ZoomFactor;
                else
                    _scale /= ZoomFactor;

                drawable.Invalidate(); // Redraw with new scale
            };



           

        var label = new Label { Text = "Hello Eto.Forms!" };

            var buildModel = new Button { Text = "Build the Mittelplate model" };
            buildModel.Click += (sender, e) => RhinoApp.RunScript("BuildModel", false);

            var importPackingList = new Button { Text = "Import Packing List" };
            importPackingList.Click += (sender, e) => RhinoApp.RunScript("ImportPackingList", false);

            var moveTDButton = new Button { Text = "Move All TDs Down" };
            moveTDButton.Click += (sender, e) => RhinoApp.RunScript("MoveAllTDToLowerPosition", false);

            var moveTDUpper = new Button { Text = "Move All TDs Upper pos" };
            moveTDUpper.Click += (sender, e) => RhinoApp.RunScript("MoveAllTDToUpperPosition", false);

            // Layout
            var layout = new DynamicLayout { Padding = 10, Spacing = new Size(5, 5) };
            layout.Add(drawable);
            layout.Add(label);
            layout.AddSeparateRow(moveTDButton);
            layout.AddSeparateRow(moveTDUpper);
            layout.AddSeparateRow(buildModel, importPackingList);

            Content = layout;

           

        }


        private void LoadCargoVisuals()
        {
            var cargoList = data.DataModelHolder.Instance.CargoList;
            if (cargoList != null)
            {
                int startX = 100;
                int startY = 100;
                int padding = 1000;

                _visualCargo.Clear(); // Just once

                foreach (var cargo in cargoList)
                {
                    for (int i = 0; i < cargo.Quentity; i++)
                    {
                        var rect = new RectangleF(startX, startY, (float)cargo.Length, (float)cargo.Width);
                        _visualCargo.Add(new CargoVisual
                        {
                            Name = cargo.Name,
                            Bounds = rect
                        });

                        startY += (int)cargo.Width + padding;
                    }
                }
            }
        }

    }
}