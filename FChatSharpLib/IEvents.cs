using System;
using FChatSharpLib.Entities.EventHandlers;

namespace FChatSharpLib
{
    public interface IEvents
    {
        event EventHandler<ReceivedPluginCommandEventArgs> ReceivedChatCommand;
        event EventHandler<ReceivedEventEventArgs> ReceivedFChatEvent;
        event EventHandler<ReceivedStateUpdateEventArgs> ReceivedStateUpdate;
        void StartListening();
        void StopListening();
        void SendCommand(string commandJson);
    }
}