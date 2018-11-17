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
            try
            {
                var command = FChatEventParser.GetParsedEvent(body, false);
                var commandType = command.GetType().Name;

                switch (commandType)
                {
                    case nameof(FChatSharpLib.Entities.Events.Client.JoinChannel):
                        var joinCommand = (FChatSharpLib.Entities.Events.Client.JoinChannel)command;
                        _bot.JoinChannel(joinCommand.channel);
                        break;
                    case nameof(FChatSharpLib.Entities.Events.Client.CreateChannel):
                        var createChanCommand = (FChatSharpLib.Entities.Events.Client.CreateChannel)command;
                        _bot.CreateChannel(createChanCommand.channel);
                        break;
                    case nameof(FChatSharpLib.Entities.Events.Client.Message):
                        var msgCommand = (FChatSharpLib.Entities.Events.Client.Message)command;
                        _bot.SendMessageInChannel(msgCommand.message, msgCommand.channel);
                        break;
                    case nameof(FChatSharpLib.Entities.Events.Client.PrivateMessage):
                        var priCommand = (FChatSharpLib.Entities.Events.Client.PrivateMessage)command;
                        _bot.SendMessageInChannel(priCommand.message, priCommand.recipient);
                        break;
                    case nameof(FChatSharpLib.Entities.Events.Client.KickFromChannel):
                        var kickCommand = (FChatSharpLib.Entities.Events.Client.KickFromChannel)command;
                        _bot.KickUser(kickCommand.character, kickCommand.channel);
                        break;
                    case nameof(FChatSharpLib.Entities.Events.Client.InviteUserToCreatedChannel):
                        var inviteChanCommand = (FChatSharpLib.Entities.Events.Client.InviteUserToCreatedChannel)command;
                        _bot.InviteUserToChannel(inviteChanCommand.character, inviteChanCommand.channel);
                        break;
                    case nameof(FChatSharpLib.Entities.Events.Client.BanFromChannel):
                        var banCommand = (FChatSharpLib.Entities.Events.Client.BanFromChannel)command;
                        _bot.BanUser(banCommand.character, banCommand.channel);
                        break;
                    case nameof(FChatSharpLib.Entities.Events.Client.ChangeChannelDescription):
                        var cdsCommand = (FChatSharpLib.Entities.Events.Client.ChangeChannelDescription)command;
                        _bot.ChangeChannelDescription(cdsCommand.channel, cdsCommand.description);
                        break;
                    case nameof(FChatSharpLib.Entities.Events.Client.ChangeChannelPrivacy):
                        var rstCommand = (FChatSharpLib.Entities.Events.Client.ChangeChannelPrivacy)command;
                        _bot.ChangeChannelPrivacy(rstCommand.channel, rstCommand.status == "private");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

        }

    }
}
