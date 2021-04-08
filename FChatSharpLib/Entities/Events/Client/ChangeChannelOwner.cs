using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Client
{
    public class ChangeChannelOwner : BaseFChatEvent
    {
        public string channel;
        public string character;

        public ChangeChannelOwner()
        {
            this.Type = "CSO";
        }
    }
}
