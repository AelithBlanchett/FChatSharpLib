using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client;

namespace FChatSharpLib.Entities.EventHandlers.WebSocket
{
    public abstract class BaseWebSocketEventHandler : IWebSocketEventHandler
    {
        private WebsocketClient _webSocketClient;

        public WebsocketClient WebSocketClient
        {
            get
            {
                return _webSocketClient;
            }

            set
            {
                _webSocketClient = value;
            }
        }

        public bool Debug { get; set; }
        public ILogger<BaseWebSocketEventHandler> Logger { get; }

        public BaseWebSocketEventHandler(ILogger<BaseWebSocketEventHandler> logger)
        {
            Logger = logger;
        }


        public void InitializeWsClient(string url, int delayBeforeReconnectInMs)
        {
            if (_webSocketClient == null)
            {
                var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
                {
                    Options =
                    {
                        KeepAliveInterval = TimeSpan.FromSeconds(3)
                    }
                });

                _webSocketClient = new WebsocketClient(new Uri(url), factory);
                ListenToWsEvents(_webSocketClient, delayBeforeReconnectInMs);
                _webSocketClient.ReconnectionHappened.Subscribe(info => OnOpen(this, null));
            }
        }

        public void ListenToWsEvents(WebsocketClient websocketClient, int delayBeforeReconnectInMs)
        {
            _webSocketClient.ReconnectTimeout = TimeSpan.FromMilliseconds(delayBeforeReconnectInMs);
            _webSocketClient.ErrorReconnectTimeout = TimeSpan.FromMilliseconds(delayBeforeReconnectInMs*4);
            _webSocketClient.DisconnectionHappened.Subscribe(type => this.OnClose(this, type));
            _webSocketClient.MessageReceived.Subscribe(type => this.OnMessage(this, type));
        }

        public abstract void OnOpen(object sender, EventArgs e);

        public abstract void OnClose(object sender, DisconnectionInfo e);

        public abstract void OnError(object sender, DisconnectionInfo e);

        public abstract void OnMessage(object sender, ResponseMessage e);

        public abstract void Close();

        public abstract void Connect();
    }
}
