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

namespace FChatSharpLib.Plugin
{
    public class RemoteBotController : IBot
    {
        private IModel _pubsubChannel;

        public Events Events { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Dictionary<string, string> ChannelsInfo { get; set; }

        public IEnumerable<string> Channels => ChannelsInfo.Keys;

        public RemoteBotController()
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
            var consumer = new EventingBasicConsumer(_pubsubChannel);
            consumer.Received += StateUpdate_Received;
            _pubsubChannel.BasicConsume(queue: "FChatSharpLib.StateUpdates",
                                 autoAck: true,
                                 consumer: consumer);
        }

        private void StateUpdate_Received(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var unparsedMessage = Encoding.UTF8.GetString(body);
            try
            {
                var deserializedObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(unparsedMessage);
                Console.WriteLine($"received: {string.Join(", ", deserializedObject.Keys)}");
                Console.WriteLine(" BasePlugin Received State Update{0}", deserializedObject);
                ChannelsInfo = deserializedObject;
            }
            catch (Exception ex)
            {
                return;
            }

        }

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
            //throw new NotImplementedException();
        }

        public void CreateChannel(string channelTitle)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            //throw new NotImplementedException();
        }

        public bool IsUserAdmin(string character, string channel)
        {
            throw new NotImplementedException();
        }

        public bool IsUserMaster(string character)
        {
            throw new NotImplementedException();
        }

        public bool IsUserOP(string character, string channel)
        {
            throw new NotImplementedException();
        }

        public void JoinChannel(string channel)
        {
            throw new NotImplementedException();
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
