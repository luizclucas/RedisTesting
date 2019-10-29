using System;

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

    }
}
