using System;
using System.IO;
using System.Threading.Tasks;
using FChatSharpLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace FChatSharp
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Async(c => c.File("Logs/logs.txt"))
                .WriteTo.Async(c => c.Console())
                .CreateLogger();

            try
            {
                Log.Information("Starting console host.");
                await CreateHostBuilder(args).RunConsoleAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly!");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }



        }

        internal static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration((context, config) =>
                {
                    //setup your additional configuration sources
                })
                .ConfigureServices((hostContext, services) =>
                {
                    FChatSharpHostOptions testOptions = new FChatSharpHostOptions();
                    hostContext.Configuration.GetSection("Options").Bind(testOptions);
                    services.Configure<FChatSharpHostOptions>(options => hostContext.Configuration.GetSection("Options").Bind(options)); // ONE SECOND

                    services.AddHostedService<FChatSharpHost>();
                });
    }
}
