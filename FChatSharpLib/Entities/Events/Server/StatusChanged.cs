using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Server
{
    public class StatusChanged : BaseFChatEvent
    {
        public string status;
        public string character;
        public string statusmsg;

        public StatusChanged()
        {
            this.Type = "STA";
        }
    }
}
