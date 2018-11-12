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
            var consumer = new EventingBasicConsumer(_pubsubChannel);
            consumer.Received += ForwardReceivedCommand;
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

            Console.WriteLine(" PluginManager Sent {0}", serializedCommand);
        }

        public void OnStateUpdate()
        {
            string serializedCommand = JsonConvert.SerializeObject(_bot.State);
            var body = Encoding.UTF8.GetBytes(serializedCommand);
            _pubsubChannel.BasicPublish(exchange: "",
                                 routingKey: "FChatSharpLib.StateUpdates",
                                 basicProperties: null,
                                 body: body);

            //Console.WriteLine(" PluginManager Sent State Update {0}", serializedCommand);
        }


        private void ForwardReceivedCommand(object model, BasicDeliverEventArgs e)
        {
            var body = Encoding.UTF8.GetString(e.Body);
            try
            {
                var command = FChatEventParser.GetParsedEvent(body, false);
                var commandType = command.GetType().Name;

                switch (commandType)
                {
                    case nameof(FChatSharpLib.Entities.Events.Client.Message):
                        var msgCommand = (FChatSharpLib.Entities.Events.Client.Message)command;
                        _bot.SendMessage(msgCommand.message, msgCommand.channel);
                        break;
                    case nameof(FChatSharpLib.Entities.Events.Client.KickFromChannel):
                        var kickCommand = (FChatSharpLib.Entities.Events.Client.KickFromChannel)command;
                        _bot.KickUser(kickCommand.character, kickCommand.channel);
                        break;
                    case nameof(FChatSharpLib.Entities.Events.Client.JoinChannel):
                        var joinCommand = (FChatSharpLib.Entities.Events.Client.JoinChannel)command;
                        _bot.JoinChannel(joinCommand.channel);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                return;
            }

        }

    }
}
