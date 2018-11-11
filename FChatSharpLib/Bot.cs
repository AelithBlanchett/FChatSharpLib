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
using FChatSharpLib.Entities.Events.Helpers;
using System.Linq;

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
        public bool IsBotReady { get; set; }

        public State State { get; set; } = new State();
        public IEnumerable<string> Channels
        {
            get
            {
                return State.ChannelsInfo.Select(x => x.Channel);
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
            switch (e.Event.GetType().Name)
            {
                case nameof(FChatSharpLib.Entities.Events.Server.JoinChannel):
                    var jchEvent = (FChatSharpLib.Entities.Events.Server.JoinChannel)e.Event;
                    State.AddCharacterInChannel(jchEvent.channel, jchEvent.character.identity);
                    PluginManager.OnStateUpdate();
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.InitialChannelData):
                    var ichEvent = (FChatSharpLib.Entities.Events.Server.InitialChannelData)e.Event;
                    foreach (var character in ichEvent.users)
                    {
                        State.AddCharacterInChannel(ichEvent.channel, character.identity);
                    }
                    PluginManager.OnStateUpdate();
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.ConnectedUsers):
                    var conEvent = (FChatSharpLib.Entities.Events.Server.ConnectedUsers)e.Event;
                    Console.WriteLine($"{conEvent.count} users connected");
                    IsBotReady = true;
                    PluginManager.OnStateUpdate();
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.ListConnectedUsers):
                    var listEvent = (FChatSharpLib.Entities.Events.Server.ListConnectedUsers)e.Event;
                    Console.WriteLine($"{listEvent.ToString()} users connected");
                    foreach (var characterState in listEvent.characters)
                    {
                        var existingState = State.CharactersInfos.Find(x => x.Character == characterState.Character);
                        if (existingState == null)
                        {
                            State.CharactersInfos.Add(characterState);
                        }
                        else
                        {
                            existingState.Gender = characterState.Gender;
                            existingState.Status = characterState.Status;
                            existingState.StatusText = characterState.StatusText;
                        }
                    }
                    PluginManager.OnStateUpdate();
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.StatusChanged):
                    var staEvent = (FChatSharpLib.Entities.Events.Server.StatusChanged)e.Event;
                    var charInfoSta = State.CharactersInfos.Find(x => x.Character == staEvent.character);
                    charInfoSta.Status = FChatEventParser.GetEnumEquivalent<StatusEnum>(staEvent.status.ToLower());
                    charInfoSta.StatusText = charInfoSta.StatusText;
                    PluginManager.OnStateUpdate();
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.OnlineNotification):
                    var nlnEvent = (FChatSharpLib.Entities.Events.Server.OnlineNotification)e.Event;
                    var charInfoNln = State.CharactersInfos.Find(x => x.Character == nlnEvent.identity);
                    if (charInfoNln == null)
                    {
                        charInfoNln = new CharacterState()
                        {
                            Character = nlnEvent.identity,
                            Gender = FChatEventParser.GetEnumEquivalent<GenderEnum>(nlnEvent.gender.ToLower()),
                            Status = FChatEventParser.GetEnumEquivalent<StatusEnum>(nlnEvent.status.ToLower())
                        };
                        State.CharactersInfos.Add(charInfoNln);
                    }
                    PluginManager.OnStateUpdate();
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.OfflineNotification):
                    var flnEvent = (FChatSharpLib.Entities.Events.Server.OfflineNotification)e.Event;
                    var charInfoFln = State.CharactersInfos.RemoveAll(x => x.Character == flnEvent.character);
                    State.ChannelsInfo.ForEach(x => x.CharactersInfo.RemoveAll(y => y.Character == flnEvent.character));
                    PluginManager.OnStateUpdate();
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.LeaveChannel):
                    var lchEvent = (FChatSharpLib.Entities.Events.Server.LeaveChannel)e.Event;
                    State.ChannelsInfo.FirstOrDefault(x => x.Channel == lchEvent.channel)?.CharactersInfo.RemoveAll(y => y.Character == lchEvent.character);
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
