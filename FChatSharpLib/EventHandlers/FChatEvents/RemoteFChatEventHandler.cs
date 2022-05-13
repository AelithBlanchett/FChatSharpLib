using FChatSharpLib.Entities.Events;
using FChatSharpLib.Entities.Events.Helpers;
using FChatSharpLib.Entities.Events.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.EventHandlers.FChatEvents
{
    public static class RemoteFChatEventHandler
    {
        public static EventHandler<ReceivedEventEventArgs> ReceivedFChatEvent;
        public static EventHandler<ReceivedPluginCommandEventArgs> ReceivedChatCommand;
        public static EventHandler<ReceivedStateUpdateEventArgs> ReceivedStateUpdate;
    }
}
