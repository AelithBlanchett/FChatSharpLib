using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Client
{
    class TypingStatus : BaseFChatEvent
    {
        public string character;
        public string status;

        public TypingStatus()
        {
            Type = "TPN";
        }
    }
}
