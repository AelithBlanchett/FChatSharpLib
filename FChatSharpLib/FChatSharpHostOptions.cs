﻿using EasyConsole;
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

        public FChatSharpHostOptions(string username, string password, string botCharacterName, string administratorCharacterName, bool debug, int delayBetweenEachReconnection, string hostname)
        {
            Username = username;
            Password = password;
            BotCharacterName = botCharacterName;
            AdministratorCharacterName = administratorCharacterName;
            Debug = debug;
            DelayBetweenEachReconnection = delayBetweenEachReconnection;
            Hostname = string.IsNullOrWhiteSpace(hostname) ? "localhost" : hostname;
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string BotCharacterName { get; set; }
        public string AdministratorCharacterName { get; set; }
        public string Hostname { get; set; }
        public bool Debug { get; set; }
        public int DelayBetweenEachReconnection { get; set; }
    }
}
