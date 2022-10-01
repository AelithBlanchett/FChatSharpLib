using EasyConsole;
using FChatSharpLib.GUI;
using FChatSharpLib.GUI.Host;
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
    public class FChatSharpHost : Program, IHostedService
    {
        public Bot Bot { get; set; }
        public static ILogger<FChatSharpHost> Logger { get; set;  }

        public FChatSharpHost(Bot bot, ILogger<FChatSharpHost> logger) : base("FChatSharp - Host", breadcrumbHeader: true)
        {
            Bot = bot;
            Logger = logger;
            Initialize();
        }

        private void Initialize()
        {
            AddPage(new MainPage(this));
            AddPage(new JoinChannelPage(this));
            AddPage(new LeaveChannelPage(this));
            AddPage(new EnableDisableDebugMode(this));
            SetPage<MainPage>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Bot.Connect();
            if (Bot.HostOptions.Value.ShowConsole)
            {
                Run();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Bot.Disconnect();
        }
    }
}
