using FChatSharpLib.Entities.Events.Helpers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace FChatSharpLib.Entities.EventHandlers.FChatEvents
{
    public static class DefaultFChatEventHandler
    {
        public static EventHandler<ReceivedEventEventArgs> ReceivedFChatEvent;
        public static EventHandler<ReceivedPluginCommandEventArgs> ReceivedChatCommand;
        public static EventHandler<ReceivedStateUpdateEventArgs> ReceivedStateUpdate;
    }
}
