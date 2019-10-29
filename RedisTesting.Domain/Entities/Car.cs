using System;

namespace RedisTesting.Domain.Entities
{
    public class Car
    {
        #region [ CONSTRUCTOR ]
        public Car(Guid carId, string autoMaker, string name, int year, double price)
        {
            CarId = carId;
            AutoMaker = autoMaker;
            Name = name;
            Year = year;
            Price = price;
        }

        public Car()
        {

        }
        #endregion

        #region [ PROPERTIES ]
        public Guid CarId { get; set; }
        public string AutoMaker { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public double Price { get; set; }
        #endregion
    }
}
