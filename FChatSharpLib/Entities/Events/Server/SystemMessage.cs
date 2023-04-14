using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Server
{
    public class SystemMessage : BaseFChatEvent
    {
        public string channel;
        public string message;

        public SystemMessage()
        {
            this.Type = "SYS";
        }
    }
}
