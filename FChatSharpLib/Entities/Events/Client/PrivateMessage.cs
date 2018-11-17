using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Client
{
    public class PrivateMessage : BaseFChatEvent
    {
        public string message;
        public string recipient;

        public PrivateMessage()
        {
            this.Type = "PRI";
        }
    }
}
