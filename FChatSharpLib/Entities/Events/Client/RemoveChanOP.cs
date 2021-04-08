using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Client
{
    public class RemoveChanOP : BaseFChatEvent
    {
        public string channel;
        public string character;

        public RemoveChanOP()
        {
            this.Type = "COR";
        }
    }
}
