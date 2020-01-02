using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;

namespace RedisTesting.Infra.Helper
{
    public static class DIProps
    {
        private static IServiceProvider _serviceProvider;

        public static IServiceProvider ServiceProvider
        {
            get
            {

                if (_serviceProvider == null)
                    throw new ApplicationException("DIProps.ServiceProvider must be set before use.");

                return _serviceProvider;
            }
            set
            {
                _serviceProvider = value;
            }
        }

        public static Props Create<TActor>() where TActor : ActorBase
        {
            return new Props(typeof(ServiceProviderActorProducer), new object[] { typeof(TActor) });
        }

        class ServiceProviderActorProducer : IIndirectActorProducer
        {
            public ServiceProviderActorProducer(Type actorType)
            {
                if (actorType == null) throw new ArgumentNullException(nameof(actorType));
                if (!actorType.IsSubclassOf(typeof(ActorBase))) throw new ArgumentException("Type must be a subclass of ActorBase.", nameof(actorType));

                ActorType = actorType;
            }

            ConcurrentDictionary<ActorBase, IServiceScope> _scopes = new ConcurrentDictionary<ActorBase, IServiceScope>();
            public Type ActorType { get; }

            public ActorBase Produce()
            {
                var scope = ServiceProvider.CreateScope();

                var test = scope.ServiceProvider.GetService(ActorType);
                var actor = (ActorBase)scope.ServiceProvider.GetService(ActorType);

                if (actor == null)
                {
                    scope.Dispose();
                    throw new InvalidOperationException($"The DI container was not able to create an actor of type '{ActorType}'. Check if it is registered.");
                }

                _scopes.GetOrAdd(actor, scope);

                return actor;
            }

            public void Release(ActorBase actor)
            {
                IServiceScope scope;

                if (_scopes.TryRemove(actor, out scope))
                {
                    scope.Dispose();
                }
            }
        }
    }
}
