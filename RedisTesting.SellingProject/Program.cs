using Akka.Actor;
using Akka.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedisTesting.Domain.Configuration;
using RedisTesting.Domain.Entities;
using RedisTesting.Domain.Entities.Request;
using RedisTesting.Domain.Entities.Response;
using RedisTesting.Infra.Data;
using RedisTesting.Infra.Helper;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedisTesting.SellingProject
{
    public partial class Program
    {
        private static ILogger _log = Log.ForContext<Program>();
        private static int _actorCount = 5;
        public static string keyToSayImChecking = "checking-request-";
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddData();

            var memoryConfig = new Dictionary<string, string>
            {
                {  "ConnectionStrings:Redis", "127.0.0.1:6379,abortConnect=false" }
            };

            var config = new ConfigurationBuilder()
                             .AddInMemoryCollection(memoryConfig)
                             .Build();

            services.AddSingleton(config);
            services.AddSingleton(ctx => ActorSystem.Create("app"));
            services.AddAllActorsFromAssemblyOf<Program>();

        }
  
        public static async Task Run()
        {
            var dataFactory = GetService<DataFactory>();
            var redis = dataFactory.GetRedisClient().GetDatabase();
            var pubAndSub = redis.Multiplexer.GetSubscriber();

            _log.Information("Running");

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


            /// Redis Lock Example.
            var system = GetService<ActorSystem>();
            var props = new RoundRobinPool(5).Props(Props.Create<Master>());
            system.ActorOf(props, "master");
            Console.ReadLine();
        }

        #region [ ACTORS ]
        public class Master : ReceiveActor
        {
            private readonly IDatabase _redis;
            private readonly DataFactory _dataFactory;
            private ICancelable _schedule;
            private readonly ILogger _log = Log.ForContext<Master>();
            private TimeSpan _timeToRun = TimeSpan.FromSeconds(5);
            private static string _redisLock = "RedisLock";
            private string _redisKeyToFinish = "FinishedProcess-";
            private readonly string myId;


            public Master()
            {
                _dataFactory = GetService<DataFactory>();
                _redis = _dataFactory.GetRedisClient().GetDatabase();
                myId = Guid.NewGuid().ToString().Substring(0,9);
                _redisKeyToFinish += myId;
                Become(Running);
            }

            void Running()
            {
                ScheduleNextRun(Run.Instance, TimeSpan.Zero);

                ReceiveAsync<Run>(async m =>
                {

                    string processing = await _redis.StringGetAsync(_redisLock);

                    var resource = "the-thing-we-are-locking-on";
                    var expiry = TimeSpan.FromSeconds(30);

                    var redLockFactory = _dataFactory.GetRedLockFactory();


                    using (var redLock = await redLockFactory.CreateLockAsync(resource, expiry)) // there are also non async Create() methods
                    {
                        // make sure we got the lock
                        if (redLock.IsAcquired)
                        {
                            if (!_redis.KeyExists(_redisKeyToFinish))
                            {
                                _log.Information("Id: {0} | I'm processing!", myId);
                                await Task.Delay(5000);
                                _log.Information("Id: {0} | I Finished!", myId);
                                await _redis.StringSetAsync(_redisKeyToFinish, true, TimeSpan.FromMinutes(5));
                            }
                        }
                        else
                        {
                            _log.Information("Id: {0} | There's someone running", myId);
                            ScheduleNextRun(Run.Instance, _timeToRun);
                        }
                    }
                });
            }

       
            void ScheduleNextRun(object message, TimeSpan delay)
            {
                //se tiver agendamento anterior, ele cancela para rodar  o novo.
                _schedule.CancelIfNotNull();
                _schedule = Context.System.Scheduler.ScheduleTellOnceCancelable(delay, Self, message, Self);
            }

            public class Run
            {
                public static readonly Run Instance = new Run();
            }
        }


        #endregion

    }
}
