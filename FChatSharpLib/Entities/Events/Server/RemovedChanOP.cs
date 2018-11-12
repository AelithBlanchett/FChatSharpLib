using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Server
{
    public class RemovedChanOP : BaseFChatEvent
    {
        public string channel;
        public string character;

        public RemovedChanOP()
        {
            this.Type = "COR";
        }
    }
}
