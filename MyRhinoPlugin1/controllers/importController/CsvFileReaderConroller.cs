using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using MyRhinoPlugin1.models;
using Rhino;

namespace MyRhinoPlugin1.controllers.importController
{
    public class CsvFileReaderController
    {
        private readonly string _filePath;

        public CsvFileReaderController(string filePath)
        {
            _filePath = filePath;
        }

        public List<CargoModel> LoadUnitsFromCsv()
        {
            List<CargoModel> cargoList = new List<CargoModel>();

            try
            {
                Debug.WriteLine($"Reading CSV: {_filePath}");

                using (var reader = new StreamReader(_filePath))
                {
                    string headerLine = reader.ReadLine(); // Read and ignore header

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        Debug.WriteLine($"Line read: {line}");

                        var values = line.Split(',');

                        Debug.WriteLine($"Parsed values: {string.Join(", ", values)}");

                        string name = values[0];
                        // Parsing numbers (replace commas with dots for decimals)
                        string qttyStr = values[1].Replace(',', '.');
                        string lengthStr = values[2].Replace(',', '.');
                        string widthStr = values[3].Replace(',', '.');
                        string heightStr = values[4].Replace(',', '.');
                        string weightStr = values[5].Replace(',', '.');

                        if (!int.TryParse(qttyStr, NumberStyles.Float, CultureInfo.InvariantCulture, out int quantity) ||
                            !double.TryParse(lengthStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double length) ||
                            !double.TryParse(widthStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double width) ||
                            !double.TryParse(heightStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double height) ||
                            !double.TryParse(weightStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double weight))
                        {
                            Debug.WriteLine($"Skipping row due to parsing error: {line}");
                            continue;
                        }

                        cargoList.Add(new CargoModel(name, quantity, length, width, height, weight));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading CSV file: {ex.Message}");
            }

            if (cargoList.Count == 0)
            {
                Debug.WriteLine("🚨 No cargo units loaded!");
            }
            else
            {
                Debug.WriteLine($"✅ Loaded {cargoList.Count} cargo units.");
            }


            foreach (var cargo in cargoList)
            {
                cargo.Length = cargo.Length * 1000; // Convert to mm
                cargo.Width = cargo.Width * 1000;   // Convert to mm
                cargo.Height = cargo.Height * 1000; // Convert to mm
            }

            return cargoList;
        }
    }
}