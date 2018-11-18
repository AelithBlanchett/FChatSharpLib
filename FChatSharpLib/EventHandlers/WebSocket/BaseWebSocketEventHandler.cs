using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace FChatSharpLib.Entities.EventHandlers.WebSocket
{
    public abstract class BaseWebSocketEventHandler : IWebSocketEventHandler
    {
        private WebSocketSharp.WebSocket _webSocketClient;

        public WebSocketSharp.WebSocket WebSocketClient
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

        public BaseWebSocketEventHandler(WebSocketSharp.WebSocket wsClient)
        {
            _webSocketClient = wsClient;
            _webSocketClient.OnOpen += this.OnOpen;
            _webSocketClient.OnClose += this.OnClose;
            _webSocketClient.OnError += this.OnError;
            _webSocketClient.OnMessage += this.OnMessage;
        }

        public abstract void OnOpen(object sender, EventArgs e);

        public abstract void OnClose(object sender, CloseEventArgs e);

        public abstract void OnError(object sender, ErrorEventArgs e);

        public abstract void OnMessage(object sender, MessageEventArgs e);

        
    }
}
