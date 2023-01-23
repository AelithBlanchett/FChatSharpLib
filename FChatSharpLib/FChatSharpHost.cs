using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FChatSharpLib
{
    public class FChatSharpHost : IHostedService
    {
        public Bot Bot { get; set; }
        public static ILogger<FChatSharpHost> Logger { get; set;  }

        public FChatSharpHost(Bot bot, ILogger<FChatSharpHost> logger)
        {
            Bot = bot;
            Logger = logger;
            Initialize();
        }

        private void Initialize()
        {
            
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Bot.Connect();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Bot.Disconnect();
        }
    }
}
