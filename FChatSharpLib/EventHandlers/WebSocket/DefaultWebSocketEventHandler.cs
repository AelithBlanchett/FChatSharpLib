using FChatSharpLib.Entities.EventHandlers.FChatEvents;
using FChatSharpLib.Entities.Events;
using FChatSharpLib.Entities.Events.Client;
using FChatSharpLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FChatSharpLib.Entities.EventHandlers.WebSocket
{
    public class DefaultWebSocketEventHandler : BaseWebSocketEventHandler
    {
        private string _webChatUrl;

        public IOptions<FChatSharpHostOptions> HostOptions { get; set; }

        private Identification _identificationInfo;

        public DefaultWebSocketEventHandler(IOptions<FChatSharpHostOptions> hostOptions)
        {
            HostOptions = hostOptions;
            Debug = HostOptions.Value.Debug;

            int port = 9722;
            if (Debug == true)
            {
                port = 8722;
            }

            _webChatUrl = $"wss://chat.f-list.net/chat2:{port}";
        }

        public override void OnClose(object sender, DisconnectionInfo e)
        {
            FChatSharpHost.Logger.LogError($"Closed connection. {e?.ToString()}.");
            FChatSharpHost.Logger.LogError($"Exception:  {e?.Exception?.ToString()}.");
            FChatSharpHost.Logger.LogWarning($"Retyring again in {HostOptions.Value.DelayBetweenEachReconnection}ms.");
        }

        public override void OnError(object sender, DisconnectionInfo e)
        {
            FChatSharpHost.Logger.LogError($"Connection closed with error. Code:  {e?.ToString()}.");
            FChatSharpHost.Logger.LogWarning($"Retyring again in {HostOptions.Value.DelayBetweenEachReconnection}ms.");
        }

        public override void OnMessage(object sender, ResponseMessage e)
        {
            if (Debug)
            {
                FChatSharpHost.Logger.LogDebug(e.Text);
            }
            FChatEventParser.HandleSpecialEvents(e.Text, DefaultFChatEventHandler.ReceivedFChatEvent, DefaultFChatEventHandler.ReceivedChatCommand);
        }

        public override void OnOpen(object sender, EventArgs e)
        {
            //Token to authenticate on F-list
            var ticket = FListClient.GetTicket(HostOptions.Value.Username, HostOptions.Value.Password);

            var identificationInfo = new Identification()
            {
                account = HostOptions.Value.Username,
                botVersion = "1.0.0",
                character = HostOptions.Value.BotCharacterName,
                ticket = ticket,
                method = "ticket",
                botCreator = HostOptions.Value.Username
            };

            WebSocketClient.Send(identificationInfo.ToString());
        }

        public override void Close()
        {
            WebSocketClient.Stop(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Closed normally.");
        }

        public override void Connect()
        {
            InitializeWsClient(_webChatUrl, HostOptions.Value.DelayBetweenEachReconnection);
            WebSocketClient.Start().Wait();
        }
    }
}
