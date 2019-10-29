using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedisTesting.Domain.Configuration;
using RedisTesting.Domain.Entities;
using RedisTesting.Domain.Entities.Request;
using RedisTesting.Domain.Entities.Response;
using RedisTesting.Infra.Data;
using RedisTesting.Infra.Helper;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedisTesting.SellingProject
{
    public partial class Program
    {
        private static ILogger _log = Log.ForContext<Program>();
        public static string keyToSayImChecking = "checking-request-";
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddData();

            var memoryConfig = new Dictionary<string, string>
            {
                {  "ConnectionStrings:Redis", "18.223.23.176:6379,abortConnect=false" }
            };

            var config = new ConfigurationBuilder()
                             .AddInMemoryCollection(memoryConfig)
                             .Build();

            services.AddSingleton(config);
        }

        public static async Task Run()
        {
            var dataFactory = GetService<DataFactory>();
            var redis = dataFactory.GetRedisClient().GetDatabase();
            var pubAndSub = redis.Multiplexer.GetSubscriber();

            await pubAndSub.SubscribeAsync(ChannelConfig.RequestCar, async (channel, message) =>
            {
                var request = message.ToString().FromJsonTo<RequestCar>();
                //Here you can have any proccess you want before checking on redis.

                string keyToCheck = keyToSayImChecking + request.Id;
                if (redis.KeyExists(keyToCheck))
                {
                    _log.Information("It's already beeing checked | request {0}", request.Id);
                }
                else
                {
                    redis.StringSet(keyToCheck, 1, TimeSpan.FromSeconds(30));
                    _log.Information("Checking request: {0}", request.Id);
                    List<Car> cars = new List<Car>();

                    for (int i = request.StartYear; i <= request.EndYear; i++)
                    {
                        string key = request.Automaker + request.Name + i;
                        if (redis.KeyExists(key))
                        {
                            var car = (await redis.StringGetAsync(key)).ToString().FromJsonTo<Car>();
                            cars.Add(car);
                        }
                    }

                    var response = new ResponseCar()
                    {
                        Cars = cars
                    };

                    await pubAndSub.PublishAsync(ChannelConfig.ResponseCar, response.ToJson());
                }
            });
        }

    }
}
