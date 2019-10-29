using Microsoft.AspNetCore.Mvc;
using RedisTesting.Domain.Entities;
using RedisTesting.Infra.Data;
using RedisTesting.Infra.Helper;
using RedisTesting.WebApi.ViewModel;
using Serilog;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace RedisTesting.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private IDatabase _redis;
        private ILogger _log = Log.ForContext<CarController>();
        public CarController(DataFactory data)
        {
            _redis = data.GetRedisClient().GetDatabase();
        }

        /// <summary>
        /// RedisKey = AutoMaker+CarName+Year | Example: FordFocus2018
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("insert")]
        public async Task<IActionResult> Insert(CarViewModel model)
        {
            string redisKey = model.AutoMaker + model.Name + model.Year;

            if (_redis.KeyExists(redisKey))
            {
                return NotFound("That car already exists!!!");
            }

            Car car = new Car(Guid.NewGuid(), model.AutoMaker, model.Name, model.Year, model.Price);

            // TTL -> Time to live, in this case it's 1 minute.
            await _redis.StringSetAsync(redisKey, car.ToJson(), TimeSpan.FromSeconds(60));
            return Ok();
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckIfExists(CarViewModel model)
        {
            string redisKey = model.AutoMaker + model.Name + model.Year;
            if (!_redis.KeyExists(redisKey))
            {
                return NotFound("That car didn't exists!!!");
            }

            var car = (await _redis.StringGetAsync(redisKey)).ToString().FromJsonTo<CarViewModel>();
            return Ok(car);
        }

    }
}
