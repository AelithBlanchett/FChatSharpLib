using System;
using System.IO;
using System.Threading.Tasks;
using FChatSharpLib;
using FChatSharpLib.Entities.EventHandlers.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;

namespace FChatSharp.Host
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Directory.CreateDirectory("logs");
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Async(c => c.File("logs/logs.txt"))
                .WriteTo.Async(c => c.Console())
                .CreateLogger();

            try
            {
                Log.Information("Starting console host.");
                await CreateHostBuilder(args).RunConsoleAsync();
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

            return 0;

        }

        internal static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration((context, config) =>
                {
                    //setup your additional configuration sources
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<FChatSharpHostOptions>(options => hostContext.Configuration.GetSection("Options").Bind(options));
                    services.Configure<ConnectionFactory>(options => hostContext.Configuration.GetSection("RabbitMQ").Bind(options));

                    services.AddHostedService<FChatSharpHost>();
                    services.AddSingleton<Bot>();
                    services.AddSingleton<Events>();
                    services.AddSingleton<IWebSocketEventHandler, DefaultWebSocketEventHandler>();
                });
    }
}
