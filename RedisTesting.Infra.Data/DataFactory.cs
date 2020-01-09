using StackExchange.Redis;
using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using RedLockNet.SERedis;
using System.Collections.Generic;
using RedLockNet.SERedis.Configuration;

namespace RedisTesting.Infra.Data
{
    public class DataFactory
    {
        private readonly Lazy<ConnectionMultiplexer> _lazyRedisClient;
        private static RedLockFactory _redLockFactory;

        public DataFactory(DataFactoryOptions options, IConfiguration config)
        {
            //Conexão com o redis.
            string redisServer = "localhost:6379";

            if (config != null)
            {
                redisServer = config.GetConnectionString(options.RedisConnectionStringName);
            }

            _lazyRedisClient = new Lazy<ConnectionMultiplexer>(() =>
            {

                var redisOptions = ConfigurationOptions.Parse(redisServer);

                redisOptions.AbortOnConnectFail = false;

                return ConnectionMultiplexer.Connect(redisOptions);

            }, LazyThreadSafetyMode.ExecutionAndPublication);


            //Criação do RedLockFactory
            var existingConnectionMultiplexer1 = ConnectionMultiplexer.Connect(redisServer);
            var multiplexers = new List<RedLockMultiplexer>{
    existingConnectionMultiplexer1};
            _redLockFactory = RedLockFactory.Create(multiplexers);
        }

        public ConnectionMultiplexer GetRedisClient() => _lazyRedisClient.Value;
        public RedLockFactory GetRedLockFactory() => _redLockFactory;

    }
}
