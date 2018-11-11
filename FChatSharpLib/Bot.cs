using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp;
using System.Net;
using Newtonsoft.Json;
using FChatSharpLib.Entities;
using FChatSharpLib.Entities.Events.Client;
using System.Collections.Specialized;
using FChatSharpLib.Entities.EventHandlers.WebSocket;
using FChatSharpLib.Entities.Plugin;
using System.Reflection;
using System.Security.Policy;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using FChatSharpLib.Entities.Events;

namespace FChatSharpLib
{
    [Serializable]
    public class Bot : IBot
    {

        private string _username;
        private string _password;
        private string _botCharacterName;
        private string _administratorCharacterName;
        private bool _debug;
        private int _delayBetweenEachReconnection;

        public IWebSocketEventHandler WSEventHandlers { get; set; }

        public WebSocket WsClient { get; set; }

        public PluginManager PluginManager { get; set; }

        public Events Events { get; set; }
        private IModel _pubsubChannel;

        public Bot(string username, string password, string botCharacterName, string administratorCharacterName)
        {
            _username = username;
            _password = password;
            _botCharacterName = botCharacterName;
            _administratorCharacterName = administratorCharacterName;
            _debug = false;
            _delayBetweenEachReconnection = 4000;
            Events = new Events();
            PluginManager = new PluginManager(this);
        }

        public Bot(string username, string password, string botCharacterName, string administratorCharacterName, bool debug, int delayBetweenEachReconnection) : this(username, password, botCharacterName, administratorCharacterName)
        {
            _debug = debug;
            _delayBetweenEachReconnection = delayBetweenEachReconnection;
        }

        private void ReceivedCommand(object model, BasicDeliverEventArgs e)
        {
            var body = Encoding.UTF8.GetString(e.Body);
            try
            {
                var command = BaseFChatEvent.Deserialize(body);
                var commandType = command.GetType().Name;

                switch (commandType)
                {
                    case "Message":
                        var typedCommand = (Message)command;
                        SendMessage(typedCommand.message, typedCommand.channel);
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

        private string GetTicket()
        {
            var jsonData = JsonConvert.SerializeObject(new
            {
                account = _username,
                password = _password
            }, Formatting.Indented);

            var jsonResult = "";
            using (WebClient wc = new WebClient())
            {
                NameValueCollection vals = new NameValueCollection();
                vals.Add("account", _username);
                vals.Add("password", _password);
                var response = wc.UploadValues("https://www.f-list.net/json/getApiTicket.php", vals);
                jsonResult = Encoding.UTF8.GetString(response);
            }

            var jsonObject = JsonConvert.DeserializeObject<GetTicketResponse>(jsonResult);

            if (string.IsNullOrEmpty(jsonObject.ticket))
            {
                throw new Exception("Couldn't get authentication info from F-List API. Please restart.");
            }
            return jsonObject.ticket;
        }

        // Connection / Disconnection

        public void Connect()
        {
            //Token to authenticate on F-list
            var ticket = GetTicket();

            int port = 9722;
            if (_debug == true)
            {
                port = 8722;
            }

            WsClient = new WebSocket($"ws://chat.f-list.net:{port}");

            var identificationInfo = new Identification()
            {
                account = _username,
                botVersion = "1.0.0",
                character = _botCharacterName,
                ticket = ticket,
                method = "ticket",
                botCreator = _username
            };

            WSEventHandlers = new DefaultWebSocketEventHandler(WsClient, identificationInfo, _delayBetweenEachReconnection);

            WsClient.Connect();

            //InitializePluginManager();
        }

        public void Disconnect()
        {
            WsClient.Close(CloseStatusCode.Normal);
        }


        





        // Channel related 

        public void JoinChannel(string channel)
        {
            WsClient.Send(new JoinChannel()
            {
                channel = channel
            }.ToString());
        }

        public void CreateChannel(string channelTitle)
        {
            WsClient.Send(new CreateChannel()
            {
                channel = channelTitle
            }.ToString());
        }

        public void SendMessage(string message, string channel)
        {
            WsClient.Send(new Message()
            {
                message = message,
                channel = channel
            }.ToString());
        }


        // Permissions / Administration

        public bool IsUserAdmin(string character, string channel)
        {
            return (this.IsUserOP(character, channel) || this.IsUserMaster(character));
        }

        public bool IsUserOP(string character, string channel)
        {
            return true;
        }

        public bool IsUserMaster(string character)
        {
            return true;
        }

        public void KickUser(string character, string channel)
        {
            WsClient.Send(new KickFromChannel()
            {
                character = character,
                channel = channel
            }.ToString());
        }

    }
}
