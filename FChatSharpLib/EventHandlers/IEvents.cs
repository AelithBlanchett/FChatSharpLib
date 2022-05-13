using System;
using FChatSharpLib.Entities.EventHandlers;
using Microsoft.Extensions.Options;

namespace FChatSharpLib
{
    public interface IEvents
    {
        double FloodLimit { get;}
        IOptions<IFChatSharpOptions> Options { get;}

        event EventHandler<ReceivedPluginCommandEventArgs> ReceivedChatCommand;
        event EventHandler<ReceivedEventEventArgs> ReceivedFChatEvent;
        event EventHandler<ReceivedStateUpdateEventArgs> ReceivedStateUpdate;
        event EventHandler<ReceivedPluginRawDataEventArgs> ReceivedPluginRawData;

        void StartListening();
        void StopListening();
        void SendCommand(string commandJson);
        void SetFloodLimit(double floodLimit);

    }
}