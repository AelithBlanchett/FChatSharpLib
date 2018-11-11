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

namespace FChatSharpLib.Entities.Plugin
{
    public class PluginManager : MarshalByRefObject
    {

        public List<PluginSpawner> PluginSpawnersList;

        //             key=channel     values=pluginName,pluginClass
        public Dictionary<string, Dictionary<string, BasePlugin>> LoadedPlugins;

        private IModel _pubsubChannel;

        public PluginManager(IBot bot)
        {

            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            _pubsubChannel = connection.CreateModel();
            _pubsubChannel.QueueDeclare(queue: "FChatLib.Plugins.FromPlugins",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            _pubsubChannel.QueueDeclare(queue: "FChatLib.Plugins.ToPlugins",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            var consumer = new EventingBasicConsumer(_pubsubChannel);
            //consumer.Received += ReceivedCommand;
            _pubsubChannel.BasicConsume(queue: "FChatLib.Plugins.FromPlugins",
                                 autoAck: true,
                                 consumer: consumer);
            Bot = bot;
        }

        public IBot Bot { get; }

        public void PassCommandToLoadedPlugins(object sender, ReceivedPluginCommandEventArgs e)
        {
            string serializedCommand = JsonConvert.SerializeObject(e);
            var body = Encoding.UTF8.GetBytes(serializedCommand);
            _pubsubChannel.BasicPublish(exchange: "",
                                 routingKey: "FChatLib.Plugins.ToPlugins",
                                 basicProperties: null,
                                 body: body);

            Console.WriteLine(" PluginManager Sent {0}", serializedCommand);
        }

        
    }
}
