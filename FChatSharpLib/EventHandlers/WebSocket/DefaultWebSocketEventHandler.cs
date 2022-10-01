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
        public new ILogger<DefaultWebSocketEventHandler> Logger { get; }

        private Identification _identificationInfo;

        public DefaultWebSocketEventHandler(IOptions<FChatSharpHostOptions> hostOptions, ILogger<DefaultWebSocketEventHandler> logger) : base(logger)
        {
            HostOptions = hostOptions;
            Logger = logger;
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
            Logger.LogError($"Closed connection. {e?.ToString()}.");
            Logger.LogError($"Exception:  {e?.Exception?.ToString()}.");
            Logger.LogWarning($"Retyring again in {HostOptions.Value.DelayBetweenEachReconnection}ms.");
        }

        public override void OnError(object sender, DisconnectionInfo e)
        {
            Logger.LogError($"Connection closed with error. Code:  {e?.ToString()}.");
            Logger.LogWarning($"Retyring again in {HostOptions.Value.DelayBetweenEachReconnection}ms.");
        }

        public override void OnMessage(object sender, ResponseMessage e)
        {
            Logger.LogDebug("New WS message received: " + e.Text);
            FChatEventParser.HandleSpecialEvents(e.Text, DefaultFChatEventHandler.ReceivedFChatEvent, DefaultFChatEventHandler.ReceivedChatCommand);
        }

        public override void OnOpen(object sender, EventArgs e)
        {
            //Token to authenticate on F-list
            Logger.LogDebug("Connecting to F-list...");
            var ticket = FListClient.GetTicket(HostOptions.Value.Username, HostOptions.Value.Password);

            Logger.LogDebug("Success! Connecting to F-chat...");

            _identificationInfo = new Identification()
            {
                account = HostOptions.Value.Username,
                botVersion = "1.0.0",
                character = HostOptions.Value.BotCharacterName,
                ticket = ticket,
                method = "ticket",
                botCreator = HostOptions.Value.Username
            };      

            WebSocketClient.Send(_identificationInfo.ToString());
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
