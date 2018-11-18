using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Server
{
    public class Ping : BaseFChatEvent
    {
        public Ping()
        {
            this.Type = "PIN";
        }
    }
}
