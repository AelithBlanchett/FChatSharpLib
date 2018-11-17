using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Server
{
    public class PrivateMessage : BaseFChatEvent
    {
        public string character;
        public string message;

        public PrivateMessage()
        {
            this.Type = "PRI";
        }
    }
}
