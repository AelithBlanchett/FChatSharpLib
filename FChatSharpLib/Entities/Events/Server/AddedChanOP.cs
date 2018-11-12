using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Server
{
    public class AddedChanOP : BaseFChatEvent
    {
        public string channel;
        public string character;

        public AddedChanOP()
        {
            this.Type = "COA";
        }
    }
}
