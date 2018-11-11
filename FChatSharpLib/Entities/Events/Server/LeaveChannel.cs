using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Server
{
    public class LeaveChannel : BaseFChatEvent
    {
        public string character;
        public string channel;

        public LeaveChannel()
        {
            this.Type = "LCH";
        }
    }
}
