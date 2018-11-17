using FChatSharpLib.Entities.Events;
using FChatSharpLib.Entities.Events.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.EventHandlers
{
    public class ReceivedStateUpdateEventArgs : EventArgs
    {
        public State State;
    }
}
