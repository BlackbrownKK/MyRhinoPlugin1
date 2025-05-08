using System;
using System.Collections.Generic;
using MyRhinoPlugin1.models;

namespace MyRhinoPlugin1.controllers.importController
{
    public class ImportController
    {

        public List<CargoModel> CargoList { get; private set; }
        public string FilePath { get; }

        public ImportController(string filePath)
        {
            FilePath = filePath;
            CsvFileReaderController csvReader = new CsvFileReaderController(FilePath);
            CargoList = csvReader.LoadUnitsFromCsv();
        }
    }
}