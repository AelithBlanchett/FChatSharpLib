using System;
using Websocket.Client;

namespace FChatSharpLib.Entities.EventHandlers.WebSocket
{
    public interface IWebSocketEventHandler
    {
        bool Debug { get; set; }
        WebsocketClient WebSocketClient { get; set; }
        void OnOpen(object sender, EventArgs e);
        void OnClose(object sender, DisconnectionInfo e);
        void OnError(object sender, DisconnectionInfo e);
        void OnMessage(object sender, ResponseMessage e);

        void Close();
        void Connect();
    }
}