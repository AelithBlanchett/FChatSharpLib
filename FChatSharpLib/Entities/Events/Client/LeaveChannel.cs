using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Client
{
    public class LeaveChannel : BaseFChatEvent
    {
        public string channel;

        public LeaveChannel()
        {
            this.Type = "LCH";
        }
    }
}
