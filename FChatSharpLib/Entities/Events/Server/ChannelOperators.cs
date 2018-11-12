using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Server
{
    public class ChannelOperators : BaseFChatEvent
    {
        public string channel;
        public List<string> oplist;

        public ChannelOperators()
        {
            this.Type = "COL";
        }
    }
}
