using FChatSharpLib.Entities;
using FChatSharpLib.Entities.EventHandlers;
using FChatSharpLib.Entities.EventHandlers.FChatEvents;
using FChatSharpLib.Entities.EventHandlers.WebSocket;
using FChatSharpLib.Entities.Events.Client;
using FChatSharpLib.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private bool debug;

        private IModel _pubsubChannel;

        private Timer _commandMonitor;
        private ConcurrentQueue<string> _commandQueue = new ConcurrentQueue<string>();

        public IWebSocketEventHandler WSEventHandlers { get; set; }
        public ILogger<IEvents> Logger { get; }

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
        public double FloodLimit { get; set; } = 3.0;

        private int ActualFloodLimit
        {
            get
            {
                return (int)(FloodLimit * 1000);
            }
        }

        public IOptions<IFChatSharpOptions> Options { get; }
        public IOptions<ConnectionFactory> RabbitMqConnectionFactory { get; }

        public Events(IOptions<FChatSharpHostOptions> hostOptions, IOptions<ConnectionFactory> rabbitMqOptions, IWebSocketEventHandler wSEventHandlers, ILogger<Events> logger)
        {
            Options = hostOptions;
            RabbitMqConnectionFactory = rabbitMqOptions;
            WSEventHandlers = wSEventHandlers;
            Logger = logger;
            Debug = Options.Value.Debug;
            ReceivedStateUpdate += OnStateUpdate;
            ReceivedFChatEvent += ForwardFChatEventsToPlugins;
            _commandMonitor = new Timer(DequeueCommandAndSendToWS, null, 0, ActualFloodLimit);
            //ReceivedChatCommand += PassChatCommandToLoadedPlugins; //Unused, see the comment in the function
        }

        public void SetFloodLimit(double floodLimit)
        {
            Logger.LogDebug("Flood limit wants to change from {0} to {1}.", FloodLimit.ToString(), floodLimit.ToString());
            FloodLimit = floodLimit;
            if(FloodLimit < 3d) { FloodLimit = 3d; }
            _commandMonitor.Change(0, ActualFloodLimit);
        }

        private void DequeueCommandAndSendToWS(object state)
        {
            if (WSEventHandlers == null || WSEventHandlers.WebSocketClient == null) {
                Logger.LogDebug("Command was skipped: WSEventHandlers haven't been initialized yet.");
                return; 
            }

            if (_commandQueue.TryDequeue(out var commandJson))
            {
                Logger.LogDebug("Command sent over WS: " + commandJson);
                if (!_commandQueue.IsEmpty)
                {
                    Logger.LogInformation($"{_commandQueue.Count} commands left to send.");
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
            var factory = RabbitMqConnectionFactory.Value;
            var connection = RabbitMqConnectionFactory.Value.CreateConnection();
            _pubsubChannel = connection.CreateModel();

            _pubsubChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            _pubsubChannel.ExchangeDeclare(exchange: "FChatSharpLib.Plugins.FromPlugins", type: ExchangeType.Fanout, durable: true);
            _pubsubChannel.ExchangeDeclare(exchange: "FChatSharpLib.Plugins.ToPlugins", type: ExchangeType.Fanout, durable: true);
            _pubsubChannel.ExchangeDeclare(exchange: "FChatSharpLib.StateUpdates", type: ExchangeType.Fanout, durable: true);
            _pubsubChannel.ExchangeDeclare(exchange: "FChatSharpLib.Events", type: ExchangeType.Fanout, durable: true);

            //F-chat commands from plugin consumer
            var queueNameState = _pubsubChannel.QueueDeclare(durable: true).QueueName;
            _pubsubChannel.QueueBind(queue: queueNameState,
                              exchange: "FChatSharpLib.Plugins.FromPlugins",
                              routingKey: "");

            var consumerState = new EventingBasicConsumer(_pubsubChannel);
            consumerState.Received += ForwardReceivedCommandToBot;
            _pubsubChannel.BasicConsume(queue: queueNameState,
                                 autoAck: true,
                                 consumer: consumerState);

            Logger.LogDebug("Initialized Pub/Sub exchanges, queues and consumers, will now connect to WS.");
            WSEventHandlers.Connect();
        }

        public void StopListening()
        {
            WSEventHandlers.Close();
            Logger.LogDebug("Closed WS connection.");
        }

        public void SendCommand(string commandJson)
        {
            Logger.LogDebug("Enqueued command: " + commandJson);
            _commandQueue.Enqueue(commandJson);
        }

        private void ForwardReceivedCommandToBot(object model, BasicDeliverEventArgs e)
        {
            var body = Encoding.UTF8.GetString(e.Body.ToArray());
            Logger.LogDebug("Received command from plugin: " + body);
            ReceivedPluginRawData?.Invoke(this, new ReceivedPluginRawDataEventArgs()
            {
                jsonData = body
            });
        }

        private void OnStateUpdate(object sender, ReceivedStateUpdateEventArgs e)
        {
            string serializedCommand = e.State.Serialize();

            var body = Encoding.UTF8.GetBytes(serializedCommand);
            Logger.LogTrace("Publishing state update with length=" + body.Length);
            _pubsubChannel.BasicPublish(exchange: "FChatSharpLib.StateUpdates",
                                 routingKey: "",
                                 basicProperties: null,
                                 body: body);
        }

        public void ForwardFChatEventsToPlugins(object sender, ReceivedEventEventArgs e)
        {
            var body = Encoding.UTF8.GetBytes(e.Event.ToString());
            Logger.LogTrace("Forwarding event to plugins: " + e.Event.ToString());
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
