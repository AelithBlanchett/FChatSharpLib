using System;
using FChatSharpLib.Entities.EventHandlers;

namespace FChatSharpLib
{
    public interface IEvents
    {
        double FloodLimit { get; set; }

        event EventHandler<ReceivedPluginCommandEventArgs> ReceivedChatCommand;
        event EventHandler<ReceivedEventEventArgs> ReceivedFChatEvent;
        event EventHandler<ReceivedStateUpdateEventArgs> ReceivedStateUpdate;
        event EventHandler<ReceivedPluginRawDataEventArgs> ReceivedPluginRawData;

        void StartListening();
        void StopListening();
        void SendCommand(string commandJson);

    }
}