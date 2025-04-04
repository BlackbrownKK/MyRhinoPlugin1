using System;

namespace MyRhinoPlugin1.models
{
    public class CargoModel
    {
        public string Name { get; set; }
        public int Quentity { get; set; } 
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }

       

        public CargoModel(string name, int quentity, double length, double width, double height, double weight)
        {
            Name = name;
            Quentity = quentity;
            Length = length;
            Width = width;
            Height = height;
            Weight = weight;
        }

        public double CalculateVolume()
        {
            return Length * Width * Height;
        }

        public override string ToString()
        {
            return $"Unit: {Name},Unit:{Quentity}, Dimensions: {Length} x {Width} x {Height}, Weight: {Weight} kg, Volume: {CalculateVolume()} cubic units";

        }
    }
}
