using System;

namespace RedisTesting.WebApi.ViewModel
{
    public class CarViewModel
    {
        public CarViewModel(Guid carId, string autoMaker, string name, int year, double price)
        {
            CarId = carId;
            AutoMaker = autoMaker;
            Name = name;
            Year = year;
            Price = price;
        }
        public CarViewModel()
        {

        }

        public Guid CarId { get; set; }
        public string AutoMaker { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public double Price { get; set; }
    }
}
