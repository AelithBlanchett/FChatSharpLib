using FChatSharpLib.Entities.Plugin;
using Newtonsoft.Json;
using RabbitMQ.Client;
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

        public RemoteBotController()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            _pubsubChannel = connection.CreateModel();
            _pubsubChannel.QueueDeclare(queue: "FChatLib.Plugins.FromPlugins",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
        }

        public void SendCommand(string commandJson)
        {
            var body = Encoding.UTF8.GetBytes(commandJson);
            _pubsubChannel.BasicPublish(exchange: "",
                                 routingKey: "FChatLib.Plugins.FromPlugins",
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
            throw new NotImplementedException();
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
