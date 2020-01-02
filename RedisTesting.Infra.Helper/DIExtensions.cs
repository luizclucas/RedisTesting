using Akka.Actor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace RedisTesting.Infra.Helper
{
    public static class DIExtensions
    {
        public static IServiceCollection AddAllActors(this IServiceCollection services, Assembly assembly)
        {
            return AddAllActors(services, assembly.GetTypes());
        }

        private static IServiceCollection AddAllActors(this IServiceCollection services, Type[] source)
        {
            var actorBaseType = typeof(ActorBase);

            var types = from t in source
                        where (t.IsPublic || t.IsNestedPublic) && t.IsSubclassOf(actorBaseType)
                        select t;

            foreach (var t in types)
            {
                services.AddTransient(t);

                var nestedTypes = t.GetNestedTypes();

                if (nestedTypes.Length > 0)
                    AddAllActors(services, nestedTypes);
            }

            return services;
        }

        public static IServiceCollection AddAllActorsFromAssemblyOf<T>(this IServiceCollection services)
        {
            return AddAllActors(services, typeof(T).Assembly);
        }

        public static TOption ConfigureOption<TOption>(this IServiceCollection services, IConfiguration configuration) where TOption : class, new()
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var config = new TOption();
            configuration.Bind(config);
            services.AddSingleton(config);

            return config;
        }
    }
}
