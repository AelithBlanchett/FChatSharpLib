using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Client
{
    public class Message : BaseFChatEvent
    {
        public string message;
        public string channel;

        public Message()
        {
            this.Type = "MSG";
        }
    }
}
