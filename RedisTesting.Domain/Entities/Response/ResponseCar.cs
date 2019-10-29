using System.Collections.Generic;

namespace RedisTesting.Domain.Entities.Response
{
    public class ResponseCar
    {
        public bool IsAvailable => Cars.Count > 0;
        public List<Car> Cars { get; set; }
    }
}
