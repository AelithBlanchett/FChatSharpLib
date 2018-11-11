using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.EventHandlers
{
    public class ReceivedPluginRequestEventArgs : EventArgs
    {
        public RequestAction Request;
    }
}
