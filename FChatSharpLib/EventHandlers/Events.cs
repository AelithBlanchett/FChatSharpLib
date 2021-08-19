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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FChatSharpLib
{
    public class Events : IEvents
    {
        private readonly string username;
        private readonly string password;
        private readonly string botCharacterName;
        private bool debug;

        private IModel _pubsubChannel;

        private Timer _commandMonitor;
        private ConcurrentQueue<string> _commandQueue = new ConcurrentQueue<string>();

        public IWebSocketEventHandler WSEventHandlers { get; set; }

        public bool Debug
        {
            get
            {
                return debug;
            }
            set
            {
                debug = value;
                if (WSEventHandlers != null)
                {
                    WSEventHandlers.Debug = value;
                }
            }
        }
        public int DelayBetweenEachReconnection { get; }
        public double FloodLimit { get; set; } = 3.0;

        private int ActualFloodLimit
        {
            get
            {
                return (int)(FloodLimit * 1000);
            }
        }

        public Events(string username, string password, string botCharacterName, bool debug, int delayBetweenEachReconnection)
        {
            this.username = username;
            this.password = password;
            this.botCharacterName = botCharacterName;
            Debug = debug;
            DelayBetweenEachReconnection = delayBetweenEachReconnection;
            ReceivedStateUpdate += OnStateUpdate;
            ReceivedFChatEvent += ForwardFChatEventsToPlugins;
            //ReceivedChatCommand += PassChatCommandToLoadedPlugins; //Unused, see the comment in the function
            _commandMonitor = new Timer(DequeueCommandAndSendToWS, null, 0, ActualFloodLimit);
        }

        public void SetFloodLimit(double floodLimit)
        {
            FloodLimit = floodLimit;
            if(FloodLimit < 2) { FloodLimit = 2d; }
            _commandMonitor.Change(0, ActualFloodLimit);
        }

        private void DequeueCommandAndSendToWS(object state)
        {
            if (WSEventHandlers == null || WSEventHandlers.WebSocketClient == null) { return; }
            if (_commandQueue.TryDequeue(out var commandJson))
            {
                if (Debug)
                {
                    Console.WriteLine("SENT: " + commandJson);
                }
                WSEventHandlers.WebSocketClient.Send(commandJson);
            }
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

            _pubsubChannel.ExchangeDeclare(exchange: "FChatSharpLib.Plugins.FromPlugins", type: ExchangeType.Fanout);
            _pubsubChannel.ExchangeDeclare(exchange: "FChatSharpLib.Plugins.ToPlugins", type: ExchangeType.Fanout);
            _pubsubChannel.ExchangeDeclare(exchange: "FChatSharpLib.StateUpdates", type: ExchangeType.Fanout);
            _pubsubChannel.ExchangeDeclare(exchange: "FChatSharpLib.Events", type: ExchangeType.Fanout);

            //F-chat commands from plugin consumer
            var queueNameState = _pubsubChannel.QueueDeclare().QueueName;
            _pubsubChannel.QueueBind(queue: queueNameState,
                              exchange: "FChatSharpLib.Plugins.FromPlugins",
                              routingKey: "");

            var consumerState = new EventingBasicConsumer(_pubsubChannel);
            consumerState.Received += ForwardReceivedCommandToBot;
            _pubsubChannel.BasicConsume(queue: queueNameState,
                                 autoAck: true,
                                 consumer: consumerState);

            WSEventHandlers = new DefaultWebSocketEventHandler(username, password, botCharacterName, DelayBetweenEachReconnection, Debug);
            WSEventHandlers.Connect();
        }

        public void StopListening()
        {
            WSEventHandlers.Close();
        }

        public void SendCommand(string commandJson)
        {
            _commandQueue.Enqueue(commandJson);
        }

        private void ForwardReceivedCommandToBot(object model, BasicDeliverEventArgs e)
        {
            var body = Encoding.UTF8.GetString(e.Body.ToArray());
            ReceivedPluginRawData?.Invoke(this, new ReceivedPluginRawDataEventArgs()
            {
                jsonData = body
            });
        }

        private void OnStateUpdate(object sender, ReceivedStateUpdateEventArgs e)
        {
            string serializedCommand = e.State.Serialize();

            var body = Encoding.UTF8.GetBytes(serializedCommand);
            _pubsubChannel.BasicPublish(exchange: "FChatSharpLib.StateUpdates",
                                 routingKey: "",
                                 basicProperties: null,
                                 body: body);
        }

        public void ForwardFChatEventsToPlugins(object sender, ReceivedEventEventArgs e)
        {
            var body = Encoding.UTF8.GetBytes(e.Event.ToString());
            _pubsubChannel.BasicPublish(exchange: "FChatSharpLib.Events",
                                 routingKey: "",
                                 basicProperties: null,
                                 body: body);
        }

        public void PassChatCommandToLoadedPlugins(object sender, ReceivedPluginCommandEventArgs e)
        {
            //There's no need to communicate this anymore, the plugins are handling the events directly by themselves.

            //string serializedCommand = JsonConvert.SerializeObject(e);
            //var body = Encoding.UTF8.GetBytes(serializedCommand);
            //_pubsubChannel.BasicPublish(exchange: "FChatSharpLib.Plugins.ToPlugins",
            //                     routingKey: "",
            //                     basicProperties: null,
            //                     body: body);
        }



    }
}
