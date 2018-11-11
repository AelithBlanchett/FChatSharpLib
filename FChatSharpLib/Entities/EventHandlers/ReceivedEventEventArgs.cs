using FChatSharpLib.Entities.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.EventHandlers
{
    class ReceivedEventEventArgs : EventArgs
    {
        public BaseFChatEvent Event;
    }
}
