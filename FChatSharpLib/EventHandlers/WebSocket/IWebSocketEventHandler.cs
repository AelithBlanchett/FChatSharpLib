using System;
using WebSocketSharp;

namespace FChatSharpLib.Entities.EventHandlers.WebSocket
{
    public interface IWebSocketEventHandler
    {
        WebSocketSharp.WebSocket WebSocketClient { get; set; }
        void OnOpen(object sender, EventArgs e);
        void OnClose(object sender, CloseEventArgs e);
        void OnError(object sender, ErrorEventArgs e);
        void OnMessage(object sender, MessageEventArgs e);
    }
}