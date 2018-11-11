using FChatSharpLib.Entities.EventHandlers;
using FChatSharpLib.Entities.EventHandlers.FChatEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib
{
    public class Events
    {
        public event EventHandler ReceivedFChatEvent
        {
            add { DefaultFChatEventHandler.ReceivedFChatEvent += value; }
            remove { DefaultFChatEventHandler.ReceivedFChatEvent -= value; }
        }

        public event EventHandler<ReceivedPluginCommandEventArgs> ReceivedPluginCommand
        {
            add { DefaultFChatEventHandler.ReceivedPluginCommandEvent += value; }
            remove { DefaultFChatEventHandler.ReceivedPluginCommandEvent -= value; }
        }




    }
}
