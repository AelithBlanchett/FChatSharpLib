using FChatSharpLib.Entities.Events;
using FChatSharpLib.Entities.Events.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.EventHandlers.FChatEvents
{
    public static class DefaultFChatEventHandler
    {
        public static EventHandler<ReceivedEventEventArgs> ReceivedFChatEvent;
        public static EventHandler<ReceivedPluginCommandEventArgs> ReceivedChatCommand;
    }
}
