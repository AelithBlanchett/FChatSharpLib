using FChatSharpLib.Entities;
using FChatSharpLib.Entities.EventHandlers;
using FChatSharpLib.Entities.EventHandlers.FChatEvents;
using FChatSharpLib.Entities.EventHandlers.WebSocket;
using FChatSharpLib.Entities.Events.Client;
using FChatSharpLib.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;


namespace FChatSharpLib
{
    public class Events : IEvents
    {
        private readonly string username;
        private readonly string password;
        private readonly string botCharacterName;
        private readonly bool debug;

        private volatile int commandsInQueue;
        private long lastTimeCommandReceived = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        private IModel _pubsubChannel;

        public IWebSocketEventHandler WSEventHandlers { get; set; }
        public WebSocket WsClient { get; set; }
        public int DelayBetweenEachReconnection { get; }
        public double FloodLimit { get; set; } = 2.0;

        public Events(string username, string password, string botCharacterName, bool debug, int delayBetweenEachReconnection)
        {
            this.username = username;
            this.password = password;
            this.botCharacterName = botCharacterName;
            this.debug = debug;
            DelayBetweenEachReconnection = delayBetweenEachReconnection;
            ReceivedStateUpdate += OnStateUpdate;
            ReceivedFChatEvent += ForwardFChatEventsToPlugins;
            ReceivedChatCommand += PassChatCommandToLoadedPlugins;
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

        public event EventHandler<ReceivedStateUpdateEventArgs> ReceivedStateUpdate
        {
            add { DefaultFChatEventHandler.ReceivedStateUpdate += value; }
            remove { DefaultFChatEventHandler.ReceivedStateUpdate -= value; }
        }

        public event EventHandler<ReceivedPluginRawDataEventArgs> ReceivedPluginRawData;


        public void StartListening()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            _pubsubChannel = connection.CreateModel();
            _pubsubChannel.QueueDeclare(queue: "FChatSharpLib.StateUpdates",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);
            _pubsubChannel.QueueDeclare(queue: "FChatSharpLib.Plugins.ToPlugins",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            _pubsubChannel.QueueDeclare(queue: "FChatSharpLib.Events",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);
            _pubsubChannel.QueueDeclare(queue: "FChatSharpLib.Plugins.FromPlugins",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(_pubsubChannel);
            consumer.Received += ForwardReceivedCommandToBot;
            _pubsubChannel.BasicConsume(queue: "FChatSharpLib.Plugins.FromPlugins",
                                 autoAck: true,
                                 consumer: consumer);
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

        public void SendCommand(string commandJson)
        {
            if (WsClient == null) { return; }
            commandsInQueue++;
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if ((currentTime - lastTimeCommandReceived) < FloodLimit)
            {
                var timeElapsedSinceLastCommand = currentTime - lastTimeCommandReceived;
                var timeToWait = (commandsInQueue * FloodLimit) - timeElapsedSinceLastCommand;
                var millisToWait = (int)(timeToWait * 1000);
                if(millisToWait < 1000) { millisToWait = 1000; }
                Thread.Sleep(millisToWait);
            }

            lastTimeCommandReceived = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            commandsInQueue--;
            WsClient.Send(commandJson);
            if (debug)
            {
                Console.WriteLine("SENT: " + commandJson);
            }
        }

        private void ForwardReceivedCommandToBot(object model, BasicDeliverEventArgs e)
        {
            var body = Encoding.UTF8.GetString(e.Body);
            ReceivedPluginRawData?.Invoke(this, new ReceivedPluginRawDataEventArgs()
            {
                jsonData = body
            });
        }

        private void OnStateUpdate(object sender, ReceivedStateUpdateEventArgs e)
        {
            string serializedCommand = e.State.Serialize();

            var body = Encoding.UTF8.GetBytes(serializedCommand);
            _pubsubChannel.BasicPublish(exchange: "",
                                 routingKey: "FChatSharpLib.StateUpdates",
                                 basicProperties: null,
                                 body: body);
        }

        public void ForwardFChatEventsToPlugins(object sender, ReceivedEventEventArgs e)
        {
            var body = Encoding.UTF8.GetBytes(e.Event.ToString());
            _pubsubChannel.BasicPublish(exchange: "",
                                 routingKey: "FChatSharpLib.Events",
                                 basicProperties: null,
                                 body: body);
        }

        public void PassChatCommandToLoadedPlugins(object sender, ReceivedPluginCommandEventArgs e)
        {
            string serializedCommand = JsonConvert.SerializeObject(e);
            var body = Encoding.UTF8.GetBytes(serializedCommand);
            _pubsubChannel.BasicPublish(exchange: "",
                                 routingKey: "FChatSharpLib.Plugins.ToPlugins",
                                 basicProperties: null,
                                 body: body);
        }



    }
}
