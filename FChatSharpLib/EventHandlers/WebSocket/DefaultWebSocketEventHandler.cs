using FChatSharpLib.Entities.EventHandlers.FChatEvents;
using FChatSharpLib.Entities.Events;
using FChatSharpLib.Entities.Events.Client;
using FChatSharpLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client;


namespace FChatSharpLib.Entities.EventHandlers.WebSocket
{
    public class DefaultWebSocketEventHandler : BaseWebSocketEventHandler
    {

        public int DelayBetweenEachReconnection { get; set; }
        private Identification _identificationInfo;
        private string _url;
        private string _username;
        private string _password;
        private string _botCharacterName;

        public DefaultWebSocketEventHandler(string username, string password, string botCharacterName, int delayBetweenEachReconnection, bool debug)
        {
            DelayBetweenEachReconnection = delayBetweenEachReconnection;
            Debug = debug;

            int port = 9722;
            if (Debug == true)
            {
                port = 8722;
            }

            _url = $"wss://chat.f-list.net/chat2:{port}";

            _username = username;
            _password = password;
            _botCharacterName = botCharacterName;
        }

        public override void OnClose(object sender, DisconnectionType e)
        {
            Console.WriteLine($"Closed connection. Code:  {e.ToString()}.");
            Console.WriteLine($"Retyring again in {DelayBetweenEachReconnection}ms.");
            System.Threading.Thread.Sleep(DelayBetweenEachReconnection);
            this.Connect();
        }

        public override void OnError(object sender, DisconnectionType e)
        {
            Console.WriteLine($"Connection closed with error. Code:  {e.ToString()}.");
            Console.WriteLine($"Retyring again in {DelayBetweenEachReconnection}ms.");
            System.Threading.Thread.Sleep(DelayBetweenEachReconnection);
            this.Connect();
        }

        public override void OnMessage(object sender, ResponseMessage e)
        {
            if (Debug)
            {
                Console.WriteLine(e.Text);
            }
            FChatEventParser.HandleSpecialEvents(e.Text, DefaultFChatEventHandler.ReceivedFChatEvent, DefaultFChatEventHandler.ReceivedChatCommand);
        }

        public override void OnOpen(object sender, EventArgs e)
        {
            //Token to authenticate on F-list
            var ticket = FListClient.GetTicket(_username, _password);

            var identificationInfo = new Identification()
            {
                account = _username,
                botVersion = "1.0.0",
                character = _botCharacterName,
                ticket = ticket,
                method = "ticket",
                botCreator = _username
            };

            WebSocketClient.Send(identificationInfo.ToString());
        }

        public override void Close()
        {
            WebSocketClient.Stop(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Closed normally.");
        }

        public override void Connect()
        {
            InitializeWsClient(_url, DelayBetweenEachReconnection);
            OnOpen(this, null);
            WebSocketClient.Start().Wait();
        }
    }
}
