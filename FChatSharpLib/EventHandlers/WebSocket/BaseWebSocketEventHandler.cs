using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
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

        public BaseWebSocketEventHandler()
        {
        }

        public void InitializeWsClient(string url, int delayBeforeReconnectInMs)
        {
            if (_webSocketClient == null)
            {
                _webSocketClient = new WebsocketClient(new Uri(url));

                _webSocketClient.ReconnectTimeoutMs = delayBeforeReconnectInMs;
                _webSocketClient.ErrorReconnectTimeoutMs = delayBeforeReconnectInMs;
                _webSocketClient.DisconnectionHappened.Subscribe(type => this.OnClose(this, type));
                _webSocketClient.MessageReceived.Subscribe(type => this.OnMessage(this, type));
            }
        }

        public abstract void OnOpen(object sender, EventArgs e);

        public abstract void OnClose(object sender, DisconnectionType e);

        public abstract void OnError(object sender, DisconnectionType e);

        public abstract void OnMessage(object sender, ResponseMessage e);

        public abstract void Close();

        public abstract void Connect();
    }
}
