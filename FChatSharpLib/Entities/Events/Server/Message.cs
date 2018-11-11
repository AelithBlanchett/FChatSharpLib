using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Server
{
    public class Message : BaseFChatEvent
    {
        public string character;
        public string message;
        public string channel;

        public Message()
        {
            this.Type = "MSG";
        }
    }
}
