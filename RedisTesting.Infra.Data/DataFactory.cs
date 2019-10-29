using StackExchange.Redis;
using System;
using System.Threading;
using Microsoft.Extensions.Configuration;


namespace RedisTesting.Infra.Data
{
    public class DataFactory
    {
        private readonly Lazy<ConnectionMultiplexer> _lazyRedisClient;
        public DataFactory(DataFactoryOptions options, IConfiguration config)
        {
            _lazyRedisClient = new Lazy<ConnectionMultiplexer>(() =>
            {
                string redisServer = "18.223.23.176:6379";

                if (config != null)
                {
                    redisServer = config.GetConnectionString(options.RedisConnectionStringName);
                }   
                var redisOptions = ConfigurationOptions.Parse(redisServer);

                redisOptions.AbortOnConnectFail = false;

                return ConnectionMultiplexer.Connect(redisOptions);

            }, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public ConnectionMultiplexer GetRedisClient() => _lazyRedisClient.Value;

    }
}
