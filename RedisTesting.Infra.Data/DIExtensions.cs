using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RedisTesting.Infra.Data
{
    public static class DIExtensions
    {
        public static void AddData(this IServiceCollection services)
        {
            services.AddDataFactory();
        }

        private static void AddDataFactory(this IServiceCollection services)
        {
            services.TryAddSingleton(sp => {

                var config = sp.GetService<IConfiguration>();
                var dataFactoryOptions = new DataFactoryOptions();
                return new DataFactory(dataFactoryOptions, config);
            });
        }

    }
}
