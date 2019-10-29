using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
            try
            {
                var services = new ServiceCollection();
                ConfigureServices(services);
                var sp = services.BuildServiceProvider();
                DIProps.ServiceProvider = RootServiceProvider = sp;

                var environmentName = Environment.CurrentDirectory;
                var bufferBaseName = Path.Combine(environmentName, "Logs", ".SeqBuffer");
                string appName = "SellingProject";

                Log.Logger = new LoggerConfiguration()
                    .Enrich.WithProperty("App",appName)
                .WriteTo.LiterateConsole()
                .WriteTo.Seq("http://seqserver.sicluster:5341", compact: true, bufferBaseFilename: bufferBaseName)
                .CreateLogger();

                Log.Information("Starting {0}", appName);
                await Run();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled Exception");

                if (Debugger.IsAttached)
                    Debugger.Break();
            }
            finally
            {
                Log.Information("Press ENTER to exit...");
                Console.ReadLine();
            }
        }

        public static T GetService<T>() => RootServiceProvider.GetRequiredService<T>();
    }
}
