using FChatSharpLib.Entities.EventHandlers;
using FChatSharpLib.Entities.EventHandlers.FChatEvents;
using FChatSharpLib.Entities.Events;
using FChatSharpLib.Entities.Events.Helpers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib
{
    public class RemoteEvents : IEvents
    {
        public double FloodLimit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IOptions<IFChatSharpOptions> Options { get; }
        public IOptions<ConnectionFactory> RabbitMqConnectionFactory { get; }

        public RemoteEvents(IOptions<FChatSharpPluginOptions> fchatSharpHostOptions, IOptions<ConnectionFactory> rabbitMqConnectionFactory)
        {
            Options = fchatSharpHostOptions;
            RabbitMqConnectionFactory = rabbitMqConnectionFactory;
        }

        private static IModel _pubsubChannel;

        public RemoteEvents()
        {
        }

        public event EventHandler<ReceivedEventEventArgs> ReceivedFChatEvent
        {
            add { RemoteFChatEventHandler.ReceivedFChatEvent += value; }
            remove { RemoteFChatEventHandler.ReceivedFChatEvent -= value; }
        }

        public event EventHandler<ReceivedPluginCommandEventArgs> ReceivedChatCommand
        {
            add { RemoteFChatEventHandler.ReceivedChatCommand += value; }
            remove { RemoteFChatEventHandler.ReceivedChatCommand -= value; }
        }

        public event EventHandler<ReceivedStateUpdateEventArgs> ReceivedStateUpdate
        {
            add { RemoteFChatEventHandler.ReceivedStateUpdate += value; }
            remove { RemoteFChatEventHandler.ReceivedStateUpdate -= value; }
        }

        public event EventHandler<ReceivedPluginRawDataEventArgs> ReceivedPluginRawData;

        public void StartListening()
        {
            var factory = RabbitMqConnectionFactory.Value;
            var connection = factory.CreateConnection();
            _pubsubChannel = connection.CreateModel();
            _pubsubChannel.ExchangeDeclare(exchange: "FChatSharpLib.Plugins.FromPlugins", type: ExchangeType.Fanout);
            _pubsubChannel.ExchangeDeclare(exchange: "FChatSharpLib.StateUpdates", type: ExchangeType.Fanout);
            _pubsubChannel.ExchangeDeclare(exchange: "FChatSharpLib.Events", type: ExchangeType.Fanout);

            //State updates consumer
            var queueNameState = _pubsubChannel.QueueDeclare().QueueName;
            _pubsubChannel.QueueBind(queue: queueNameState,
                              exchange: "FChatSharpLib.StateUpdates",
                              routingKey: "");
            var consumerState = new EventingBasicConsumer(_pubsubChannel);
            consumerState.Received += StateUpdate_Received;
            _pubsubChannel.BasicConsume(queue: queueNameState,
                                 autoAck: true,
                                 consumer: consumerState);

            //Events consumer
            var queueNameEvents = _pubsubChannel.QueueDeclare().QueueName;
            _pubsubChannel.QueueBind(queue: queueNameEvents,
                              exchange: "FChatSharpLib.Events",
                              routingKey: "");
            var consumerEvents = new EventingBasicConsumer(_pubsubChannel);
            consumerEvents.Received += RelayServerEvents;
            _pubsubChannel.BasicConsume(queue: queueNameEvents,
                                 autoAck: true,
                                 consumer: consumerEvents);
        }

        public void StopListening()
        {
            _pubsubChannel.Close();
            _pubsubChannel.Dispose();
            _pubsubChannel = null;
        }

        public void SendCommand(string commandJson)
        {
            if (_pubsubChannel != null)
            {
                var body = Encoding.UTF8.GetBytes(commandJson);
                _pubsubChannel.BasicPublish(exchange: "FChatSharpLib.Plugins.FromPlugins",
                                     routingKey: "",
                                     basicProperties: null,
                                     body: body);
            }
        }

        public void SetFloodLimit(double floodLimit)
        {
            //throw new NotImplementedException();
        }

        private static void RelayServerEvents(object sender, BasicDeliverEventArgs e)
        {
            var body = Encoding.UTF8.GetString(e.Body.ToArray());
            try
            {
                FChatEventParser.HandleSpecialEvents(body, RemoteFChatEventHandler.ReceivedFChatEvent, RemoteFChatEventHandler.ReceivedChatCommand);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static void StateUpdate_Received(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var unparsedMessage = Encoding.UTF8.GetString(body.ToArray());
            try
            {
                var deserializedObject = JsonConvert.DeserializeObject<State>(unparsedMessage);
                RemoteFChatEventHandler.ReceivedStateUpdate?.Invoke(null, new ReceivedStateUpdateEventArgs()
                {
                    State = deserializedObject
                });
            }
            catch (Exception ex)
            {
                return;
            }
        }
    }
}
