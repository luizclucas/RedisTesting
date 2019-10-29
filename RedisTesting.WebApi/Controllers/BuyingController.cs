using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RedisTesting.Domain.Configuration;
using RedisTesting.Domain.Entities.Request;
using RedisTesting.Domain.Entities.Response;
using RedisTesting.Infra.Data;
using RedisTesting.Infra.Helper;
using Serilog;
using StackExchange.Redis;

namespace RedisTesting.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyingController : ControllerBase
    {
        private ISubscriber _pubAndSub;
        private ILogger _log = Log.ForContext<BuyingController>();
        public BuyingController(DataFactory data)
        {
            _pubAndSub = data.GetRedisClient().GetDatabase().Multiplexer.GetSubscriber();
        }

        #region [ ACTIONS ]
        [HttpPost("requestcar")]
        public async Task<IActionResult> GetAvailableCars([FromBody] RequestCar requestCar)
        {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            CancellationTokenSource cts = new CancellationTokenSource(10000);
            cts.Token.Register(() => tcs.TrySetResult(0));
            ResponseCar response = new ResponseCar();

            await _pubAndSub.SubscribeAsync(ChannelConfig.ResponseCar, (channel, message) =>
            {
                response = message.ToString().FromJsonTo<ResponseCar>();
                tcs.TrySetResult(1);
            });

            requestCar.Id = Guid.NewGuid();
            await _pubAndSub.PublishAsync(ChannelConfig.RequestCar, requestCar.ToJson());

            var result = await tcs.Task;
            if (result <= 0)
            {
                _log.Information("Timeout when trying to request a car");
                return NotFound("Not possible to connect with the service.");
            }
            return Ok(response.ToJson());
        }
        #endregion

    }
}