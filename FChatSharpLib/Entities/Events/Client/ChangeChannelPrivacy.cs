using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Client
{
    class ChangeChannelPrivacy : BaseFChatEvent
    {
        public string channel;
        public string status;

        public ChangeChannelPrivacy()
        {
            Type = "RST";
        }
    }
}
