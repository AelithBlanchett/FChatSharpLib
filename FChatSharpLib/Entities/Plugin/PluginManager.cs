using FChatSharpLib.Entities.EventHandlers;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using FChatSharpLib.Entities.Events;
using FChatSharpLib.Entities.Events.Server;

namespace FChatSharpLib.Entities.Plugin
{
    public class PluginManager
    {

        private IModel _pubsubChannel;
        private IBot _bot;

        public PluginManager(IBot bot)
        {
            _bot = bot;
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            _pubsubChannel = connection.CreateModel();
            _pubsubChannel.QueueDeclare(queue: "FChatSharpLib.Plugins.FromPlugins",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            _pubsubChannel.QueueDeclare(queue: "FChatSharpLib.Plugins.ToPlugins",
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
            var consumer = new EventingBasicConsumer(_pubsubChannel);
            consumer.Received += ForwardReceivedCommandToBot;
            _pubsubChannel.BasicConsume(queue: "FChatSharpLib.Plugins.FromPlugins",
                                 autoAck: true,
                                 consumer: consumer);

            
        }

        public void PassCommandToLoadedPlugins(object sender, ReceivedPluginCommandEventArgs e)
        {
            string serializedCommand = JsonConvert.SerializeObject(e);
            var body = Encoding.UTF8.GetBytes(serializedCommand);
            _pubsubChannel.BasicPublish(exchange: "",
                                 routingKey: "FChatSharpLib.Plugins.ToPlugins",
                                 basicProperties: null,
                                 body: body);
        }

        public void ForwardFChatEventsToPlugin(object sender, ReceivedEventEventArgs e)
        {
            var body = Encoding.UTF8.GetBytes(e.Event.ToString());
            _pubsubChannel.BasicPublish(exchange: "",
                                 routingKey: "FChatSharpLib.Events",
                                 basicProperties: null,
                                 body: body);
        }

        public void OnStateUpdate()
        {
            string serializedCommand = _bot.State.Serialize();

            var body = Encoding.UTF8.GetBytes(serializedCommand);
            _pubsubChannel.BasicPublish(exchange: "",
                                 routingKey: "FChatSharpLib.StateUpdates",
                                 basicProperties: null,
                                 body: body);
        }


        private void ForwardReceivedCommandToBot(object model, BasicDeliverEventArgs e)
        {
            var body = Encoding.UTF8.GetString(e.Body);
            _bot.SendCommandToServer(body);
        }

    }
}
