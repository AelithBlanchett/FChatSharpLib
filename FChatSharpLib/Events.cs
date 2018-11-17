using FChatSharpLib.Entities;
using FChatSharpLib.Entities.EventHandlers;
using FChatSharpLib.Entities.EventHandlers.FChatEvents;
using FChatSharpLib.Entities.EventHandlers.WebSocket;
using FChatSharpLib.Entities.Events.Client;
using FChatSharpLib.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;


namespace FChatSharpLib
{
    public class Events : IEvents
    {
        private Identification identificationInfo;
        private readonly string username;
        private readonly string password;
        private readonly string botCharacterName;
        private readonly bool debug;

        private int commandsInQueue;
        private long lastTimeCommandReceived = long.MaxValue;
        private double floodLimit = 2.0;

        public IWebSocketEventHandler WSEventHandlers { get; set; }
        public WebSocket WsClient { get; set; }
        public int DelayBetweenEachReconnection { get; }

        public Events(string username, string password, string botCharacterName, bool debug, int delayBetweenEachReconnection)
        {
            this.username = username;
            this.password = password;
            this.botCharacterName = botCharacterName;
            this.debug = debug;
            DelayBetweenEachReconnection = delayBetweenEachReconnection;
        }

        public event EventHandler<ReceivedEventEventArgs> ReceivedFChatEvent
        {
            add { DefaultFChatEventHandler.ReceivedFChatEvent += value; }
            remove { DefaultFChatEventHandler.ReceivedFChatEvent -= value; }
        }

        public event EventHandler<ReceivedPluginCommandEventArgs> ReceivedChatCommand
        {
            add { DefaultFChatEventHandler.ReceivedChatCommand += value; }
            remove { DefaultFChatEventHandler.ReceivedChatCommand -= value; }
        }

        public event EventHandler<ReceivedStateUpdateEventArgs> ReceivedStateUpdate;

        public void StartListening()
        {
            //Token to authenticate on F-list
            var ticket = FListClient.GetTicket(username, password);

            var identificationInfo = new Identification()
            {
                account = username,
                botVersion = "1.0.0",
                character = botCharacterName,
                ticket = ticket,
                method = "ticket",
                botCreator = username
            };

            int port = 9722;
            if (debug == true)
            {
                port = 8722;
            }

            WsClient = new WebSocket($"ws://chat.f-list.net:{port}");

            WSEventHandlers = new DefaultWebSocketEventHandler(WsClient, identificationInfo, DelayBetweenEachReconnection);

            WsClient.Connect();
        }

        public void StopListening()
        {
            WsClient.Close(CloseStatusCode.Normal);
        }

        public async void SendCommand(string commandJson)
        {
            if(WsClient == null) { return; }
            commandsInQueue++;
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if ((currentTime - lastTimeCommandReceived) < floodLimit)
            {
                var timeElapsedSinceLastCommand = currentTime - lastTimeCommandReceived;
                var timeToWait = (commandsInQueue * floodLimit) - timeElapsedSinceLastCommand;
                await Task.Delay((int)timeToWait * 1000);
            }

            lastTimeCommandReceived = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            commandsInQueue--;
            WsClient.Send(commandJson);
            if (debug)
            {
                Console.WriteLine("SENT: " + commandJson);
            }
        }



    }
}
