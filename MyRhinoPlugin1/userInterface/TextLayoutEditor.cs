using Eto.Drawing;
using Eto.Forms;
using Eto.Forms;
using MyRhinoPlugin1.commands.layout;
using MyRhinoPlugin1.data;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRhinoPlugin1.userInterface
{
    public class TextLayoutEditor : Form
    {

        public List<AttributeRow> EditedAttributes { get; private set; }

 
 

        public TextLayoutEditor()
        {
            Title = "Edit Attributes";
            ClientSize = new Size(500, 600);
            Resizable = false;

            var grid = new GridView<AttributeRow>
            {
                Columns =
                {
                    new GridColumn
                    {
                        HeaderText = "Tag",
                        DataCell = new TextBoxCell { Binding = Binding.Property<AttributeRow, string>(r => r.Tag) },
                        Editable = false,
                        Width = 150
                    },
                    new GridColumn
                    {
                        HeaderText = "Value",
                        DataCell = new TextBoxCell { Binding = Binding.Property<AttributeRow, string>(r => r.Value) },
                        Editable = true,
                        Width = 300
                    }
                },
                Height = 500
            };


            // Sample data
            var data = new List<AttributeRow>
            {
                new AttributeRow("VesselName", DataModelHolder.VesselName),
                new AttributeRow("VesselType", DataModelHolder.VesselType), 
                new AttributeRow("titel", DataModelHolder.titel),
                new AttributeRow("subject", DataModelHolder.subject),
                new AttributeRow("rev", DataModelHolder.rev),
                new AttributeRow("planner", DataModelHolder.planner),
                new AttributeRow("planType", DataModelHolder.planType),
                new AttributeRow("planStatus", DataModelHolder.planStatus),
                new AttributeRow("jobId", DataModelHolder.jobId),
                new AttributeRow("Date", DataModelHolder.Date)
            };

            grid.DataStore = data;

  
            // Buttons
            var okButton = new Button { Text = "OK" };
            okButton.Click += (sender, e) =>
            {
                EditedAttributes = data; // Save the current edited data
                EditedAttributes = data;

                var type = typeof(DataModelHolder);

                foreach (var row in EditedAttributes)
                {
                    // Try to get the property by the tag name (must match exactly!)
                    var prop = type.GetProperty(row.Tag, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(null, row.Value); // Set static property
                    }
                }
                var doc = Rhino.RhinoDoc.ActiveDoc;
                LayoutMaker.textUpdator(doc);
                doc.Views.Redraw();

                Close();
            };

            var cancelButton = new Button { Text = "Cancel" };
            cancelButton.Click += (sender, e) => Close();

            var resetButton = new Button { Text = "Reset" };
            resetButton.Click += (sender, e) =>
            {
                // Reset logic placeholder
                foreach (var row in data)
                    row.Value = ""; // Clear or reload original
                grid.ReloadData(1);
                grid.ReloadData(2);

            };

            var buttonLayout = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                Items =
                {
                    resetButton,
                    new StackLayoutItem(null, true), // spacer
                    okButton,
                    cancelButton
                }
            };

            Content = new StackLayout
            {
                Spacing = 10,
                Padding = new Padding(10),
                Items =
        {
            new Label
            {
                Text = "Block Name: Title Block Page 1",
              Font = SystemFonts.Default(12)
            },
            grid,
            buttonLayout
        }
            };
        }

        public class AttributeRow
        {
            public string Tag { get; set; }
            public string Value { get; set; }

            public AttributeRow(string tag, string value)
            {
                Tag = tag;
                Value = value;
            }
        }
    }
}