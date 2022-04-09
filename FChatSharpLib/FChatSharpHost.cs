using EasyConsole;
using FChatSharpLib.GUI;
using FChatSharpLib.GUI.Host;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FChatSharpLib
{
    public class FChatSharpHost : Program, IHostedService
    {
        public Bot Bot;

        public string Username { get; }
        public string Password { get; }
        public string BotCharacterName { get; }
        public string AdministratorCharacterName { get; }
        public string Hostname { get; }
        public bool Debug { get; }
        public int DelayBetweenEachReconnection { get; }
        public static ILogger<FChatSharpHost> Logger { get; set;  }

        public FChatSharpHost(IOptions<FChatSharpHostOptions> botOptions, ILogger<FChatSharpHost> logger, IHostApplicationLifetime appLifetime) : base("FChatSharp - Host", breadcrumbHeader: true)
        {
            var options = botOptions.Value;
            Username = options.Username;
            Password = options.Password;
            BotCharacterName = options.BotCharacterName;
            AdministratorCharacterName = options.AdministratorCharacterName;
            Hostname = options.Hostname;
            Debug = options.Debug;
            DelayBetweenEachReconnection = options.DelayBetweenEachReconnection;
            Initialize();
            Logger = logger;
        }

        public FChatSharpHost(string username, string password, string botCharacterName, string administratorCharacterName, bool debug, int delayBetweenEachReconnection) : base("FChatSharp - Host", breadcrumbHeader: true)
        {
            Username = username;
            Password = password;
            BotCharacterName = botCharacterName;
            AdministratorCharacterName = administratorCharacterName;
            Debug = debug;
            DelayBetweenEachReconnection = delayBetweenEachReconnection;
            Initialize();
        }
        private void Initialize()
        {
            AddPage(new MainPage(this));
            AddPage(new JoinChannelPage(this));
            AddPage(new LeaveChannelPage(this));
            AddPage(new EnableDisableDebugMode(this));
            SetPage<MainPage>();
            Bot = new Bot(Username, Password, BotCharacterName, AdministratorCharacterName, Debug, DelayBetweenEachReconnection);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Bot.Connect();
            Run();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Bot.Disconnect();
        }
    }
}
