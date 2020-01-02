using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RedisTesting.Infra.Helper;
using Serilog;

namespace RedisTesting.SellingProject
{
    public partial class Program
    {
        public static IServiceProvider RootServiceProvider;

        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
              .WriteTo.LiterateConsole()
              .WriteTo.Seq("http://seqserver.sicluster:5341", compact: true)
              .CreateLogger();

            var services = new ServiceCollection();
            ConfigureServices(services);
            var sp = services.BuildServiceProvider();
            DIProps.ServiceProvider = RootServiceProvider = sp;
            await Run();            
        }

        public static T GetService<T>() => RootServiceProvider.GetRequiredService<T>();
    }
}
