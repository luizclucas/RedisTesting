using System;
using System.Diagnostics;

namespace RedisTesting.Domain.Entities.Request
{
    public class RequestCar
    {
        public Guid Id { get; set; }
        public string Automaker { get; set; }
        public string Name { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
    }
}
