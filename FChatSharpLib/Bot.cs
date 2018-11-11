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
using System.Threading;
using System.Threading.Tasks;

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
        private Timer _pingTimer;
        private int commandsInQueue;
        private long lastTimeCommandReceived = long.MaxValue;
        private double floodLimit = 2.0;

        public IWebSocketEventHandler WSEventHandlers { get; set; }
        public WebSocket WsClient { get; set; }
        public PluginManager PluginManager { get; set; }
        public Events Events { get; set; }
        public Dictionary<string, string> ChannelsInfo { get; set; }
        public IEnumerable<string> Channels
        {
            get
            {
                return ChannelsInfo.Keys;
            }
        }




        public Bot(string username, string password, string botCharacterName, string administratorCharacterName)
        {
            _username = username;
            _password = password;
            _botCharacterName = botCharacterName;
            _administratorCharacterName = administratorCharacterName;
            _debug = false;
            _delayBetweenEachReconnection = 4000;
            Events = new Events();
            ChannelsInfo = new Dictionary<string, string>();
            PluginManager = new PluginManager(this);
        }

        public Bot(string username, string password, string botCharacterName, string administratorCharacterName, bool debug, int delayBetweenEachReconnection) : this(username, password, botCharacterName, administratorCharacterName)
        {
            _debug = debug;
            _delayBetweenEachReconnection = delayBetweenEachReconnection;
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

            _pingTimer = new Timer(SendPing, null, 5000, 5000);
            Events.ReceivedFChatEvent += Events_ReceivedFChatEvent;
            Events.ReceivedChatCommand += PluginManager.PassCommandToLoadedPlugins;
        }


        private void Events_ReceivedFChatEvent(object sender, Entities.EventHandlers.ReceivedEventEventArgs e)
        {
            Console.WriteLine(e.ToString());
            switch (e.Event.GetType().Name)
            {
                case nameof(FChatSharpLib.Entities.Events.Server.JoinChannel):
                    var jchEvent = (FChatSharpLib.Entities.Events.Server.JoinChannel)e.Event;
                    ChannelsInfo.TryAdd(jchEvent.channel, jchEvent.character.ToString());
                    PluginManager.OnStateUpdate();
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.InitialChannelData):
                    var ichEvent = (FChatSharpLib.Entities.Events.Server.InitialChannelData)e.Event;
                    foreach (var identity in ichEvent.users)
                    {
                        ChannelsInfo.TryAdd(ichEvent.channel, identity.ToString());
                    }
                    PluginManager.OnStateUpdate();
                    break;
                default:
                    break;
            }
        }

        public void Disconnect()
        {
            WsClient.Close(CloseStatusCode.Normal);
        }

        public void SendPing(Object stateInfo)
        {
            SendWsMessage(new Ping()
            {
            }.ToString());
        }







        // Channel related 

        public void JoinChannel(string channel)
        {
            SendWsMessage(new JoinChannel()
            {
                channel = channel
            }.ToString());
        }

        public void CreateChannel(string channelTitle)
        {
            SendWsMessage(new CreateChannel()
            {
                channel = channelTitle
            }.ToString());
        }

        public void SendMessage(string message, string channel)
        {
            SendWsMessage(new Message()
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
            return character.ToLower() == _administratorCharacterName;
        }

        public void KickUser(string character, string channel)
        {
            SendWsMessage(new KickFromChannel()
            {
                character = character,
                channel = channel
            }.ToString());
        }

        private async void SendWsMessage(string data)
        {
            commandsInQueue++;
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if ((currentTime - lastTimeCommandReceived) < floodLimit)
            {
                var timeElapsedSinceLastCommand = currentTime - lastTimeCommandReceived;
                var timeToWait = (commandsInQueue * floodLimit) - timeElapsedSinceLastCommand;
                await Task.Delay((int)timeToWait * 1000);
            }

            lastTimeCommandReceived = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            commandsInQueue--;
            WsClient.Send(data);
        }

    }
}
