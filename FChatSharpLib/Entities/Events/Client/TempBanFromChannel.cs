using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Client
{
    class TempBanFromChannel : BaseFChatEvent
    {
        public string channel;
        public string character;
        public string length;

        public TempBanFromChannel()
        {
            Type = "CTU";
        }
    }
}
