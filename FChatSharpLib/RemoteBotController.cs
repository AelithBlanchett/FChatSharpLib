using FChatSharpLib.Entities.EventHandlers;
using FChatSharpLib.Entities.Events;
using FChatSharpLib.Entities.Events.Helpers;
using FChatSharpLib.Entities.Plugin;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace FChatSharpLib
{
    public class RemoteBotController : IBot
    {
        private IModel _pubsubChannel;

        public Events Events { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IEnumerable<string> Channels => State.ChannelsInfo.Select(x => x.Key);
        public State State { get; set; }

        public RemoteBotController()
        {
        }

        //Events

        private void RelayServerEvents(object sender, BasicDeliverEventArgs e)
        {
            var body = Encoding.UTF8.GetString(e.Body);
            try
            {
                var command = FChatEventParser.GetParsedEvent(body, true);
                ReceivedFChatEvent?.Invoke(sender, new ReceivedEventEventArgs()
                {
                    Event = command
                });
                switch (command.GetType().Name)
                {
                    case nameof(Entities.Events.Server.JoinChannel):
                        UserJoinedChannel?.Invoke(this, (Entities.Events.Server.JoinChannel)command);
                        break;
                    default:
                        break;
                }
            }
            catch(Exception ex)
            {
                return;
            }
        }

        private void StateUpdate_Received(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var unparsedMessage = Encoding.UTF8.GetString(body);
            try
            {
                var deserializedObject = JsonConvert.DeserializeObject<State>(unparsedMessage);
                State = deserializedObject;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public EventHandler<ReceivedEventEventArgs> ReceivedFChatEvent;

        public event EventHandler<Entities.Events.Server.JoinChannel> UserJoinedChannel;



        //Misc

        public void SendCommand(string commandJson)
        {
            var body = Encoding.UTF8.GetBytes(commandJson);
            _pubsubChannel.BasicPublish(exchange: "",
                                 routingKey: "FChatSharpLib.Plugins.FromPlugins",
                                 basicProperties: null,
                                 body: body);
        }

        public void Connect()
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

        public void Disconnect()
        {
            _pubsubChannel.Close();
            _pubsubChannel.Dispose();
            _pubsubChannel = null;
        }



        //Permissions

        public bool IsUserAdmin(string character, string channel)
        {
            return (this.IsUserOP(character, channel) || this.IsUserMaster(character));
        }

        public bool IsUserMaster(string character)
        {
            return character.ToLower() == State.AdminCharacterName.ToLower();
        }

        public bool IsSelf(string character)
        {
            return character.ToLower() == State.BotCharacterName.ToLower();
        }

        public bool IsUserOP(string character, string channel)
        {
            return (State.ChannelsInfo.GetValueOrDefault(channel.ToLower()) != null ? State.ChannelsInfo.GetValueOrDefault(channel.ToLower()).Operators.Any( x=> x.ToLower() == character.ToLower()) : false);
        }



        //Bot commands

        public void CreateChannel(string channelTitle)
        {
            var cchCommand = new FChatSharpLib.Entities.Events.Client.CreateChannel()
            {
                channel = channelTitle
            };
            SendCommand(cchCommand.ToString());
        }

        public void JoinChannel(string channel)
        {
            var jchCommand = new FChatSharpLib.Entities.Events.Client.JoinChannel()
            {
                channel = channel
            };
            SendCommand(jchCommand.ToString());
        }

        public void KickUser(string character, string channel)
        {
            var kickCommand = new FChatSharpLib.Entities.Events.Client.KickFromChannel()
            {
                character = character,
                channel = channel
            };
            SendCommand(kickCommand.ToString());
        }

        public void SendMessage(string message, string channel)
        {
            var messageCommand = new FChatSharpLib.Entities.Events.Client.Message()
            {
                message = message,
                channel = channel
            };
            SendCommand(messageCommand.ToString());
        }
    }
}
