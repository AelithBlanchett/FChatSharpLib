using FChatSharpLib.Entities.Events;
using FChatSharpLib.Entities.Events.Helpers;
using FChatSharpLib.Entities.Events.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.EventHandlers.FChatEvents
{
    public static class RemoteFChatEventHandler
    {
        public static EventHandler<ReceivedEventEventArgs> ReceivedFChatEvent;
        public static EventHandler<ReceivedPluginCommandEventArgs> ReceivedChatCommand;
        public static EventHandler<ReceivedStateUpdateEventArgs> ReceivedStateUpdate;
        private static IModel _pubsubChannel;

        public static void Connect()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            _pubsubChannel = connection.CreateModel();
            _pubsubChannel.QueueDeclare(queue: "FChatSharpLib.Plugins.FromPlugins",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
            _pubsubChannel.QueueDeclare(queue: "FChatSharpLib.StateUpdates",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
            _pubsubChannel.QueueDeclare(queue: "FChatSharpLib.Events",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
            var consumerState = new EventingBasicConsumer(_pubsubChannel);
            consumerState.Received += StateUpdate_Received;
            _pubsubChannel.BasicConsume(queue: "FChatSharpLib.StateUpdates",
                                 autoAck: true,
                                 consumer: consumerState);
            var consumerEvents = new EventingBasicConsumer(_pubsubChannel);
            consumerEvents.Received += RelayServerEvents;
            _pubsubChannel.BasicConsume(queue: "FChatSharpLib.Events",
                                 autoAck: true,
                                 consumer: consumerEvents);
        }

        public static void SendJsonCommand(string commandJson)
        {
            if(_pubsubChannel != null)
            {
                var body = Encoding.UTF8.GetBytes(commandJson);
                _pubsubChannel.BasicPublish(exchange: "",
                                     routingKey: "FChatSharpLib.Plugins.FromPlugins",
                                     basicProperties: null,
                                     body: body);
            }
            
        }

        public static void Disconnect()
        {
            _pubsubChannel.Close();
            _pubsubChannel.Dispose();
            _pubsubChannel = null;
        }

        public static void RelayEvent()
        {

        }

        private static void RelayServerEvents(object sender, BasicDeliverEventArgs e)
        {
            var body = Encoding.UTF8.GetString(e.Body);
            try
            {
                FChatEventParser.HandleSpecialEvents(body, ReceivedFChatEvent, ReceivedChatCommand);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static void StateUpdate_Received(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var unparsedMessage = Encoding.UTF8.GetString(body);
            try
            {
                var deserializedObject = JsonConvert.DeserializeObject<State>(unparsedMessage);
                ReceivedStateUpdate?.Invoke(null, new ReceivedStateUpdateEventArgs()
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
