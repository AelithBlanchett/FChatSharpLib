using EasyConsole;
using FChatSharpLib.GUI;
using FChatSharpLib.GUI.Host;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FChatSharpLib
{
    public class FChatSharpHostOptions : IFChatSharpOptions
    {
        public FChatSharpHostOptions()
        {

        }

        public FChatSharpHostOptions(string username, string password, string botCharacterName, string administratorCharacterName, bool debug, int delayBetweenEachReconnection, bool showConsole)
        {
            Username = username;
            Password = password;
            BotCharacterName = botCharacterName;
            AdministratorCharacterName = administratorCharacterName;
            Debug = debug;
            ShowConsole = showConsole;
            DelayBetweenEachReconnection = delayBetweenEachReconnection == 0 ? 4000 : delayBetweenEachReconnection;

            if (string.IsNullOrWhiteSpace(Username))
            {
                throw new InvalidOperationException($"The {nameof(Username)} value in the configuration file is invalid: {Username}");
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                throw new InvalidOperationException($"The {nameof(Password)} value in the configuration file is invalid: {Password}");
            }
            if (string.IsNullOrWhiteSpace(AdministratorCharacterName))
            {
                throw new InvalidOperationException($"The {nameof(AdministratorCharacterName)} value in the configuration file is invalid: {AdministratorCharacterName}");
            }
            if (string.IsNullOrWhiteSpace(BotCharacterName))
            {
                throw new InvalidOperationException($"The {nameof(BotCharacterName)} value in the configuration file is invalid: {BotCharacterName}");
            }
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string BotCharacterName { get; set; }
        public string AdministratorCharacterName { get; set; }
        public bool Debug { get; set; } = false;
        public bool ShowConsole { get; }
        public int DelayBetweenEachReconnection { get; set; }
    }
}
