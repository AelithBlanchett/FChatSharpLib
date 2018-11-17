using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Client
{
    public class ChangeChannelDescription : BaseFChatEvent
    {
        public string channel;
        public string description;

        public ChangeChannelDescription()
        {
            Type = "CDS";
        }
    }
}
