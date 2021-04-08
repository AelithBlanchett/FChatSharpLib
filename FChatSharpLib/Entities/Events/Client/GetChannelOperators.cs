using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Client
{
    public class GetChannelOperators : BaseFChatEvent
    {
        public string channel;

        public GetChannelOperators()
        {
            this.Type = "COL";
        }
    }
}
