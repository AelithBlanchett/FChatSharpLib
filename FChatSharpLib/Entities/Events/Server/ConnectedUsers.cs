using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Server
{
    public class ConnectedUsers : BaseFChatEvent
    {
        public int count;

        public ConnectedUsers()
        {
            this.Type = "CON";
        }
    }
}
