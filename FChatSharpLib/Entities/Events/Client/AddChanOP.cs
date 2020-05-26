using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Client
{
    class AddChanOP : BaseFChatEvent
    {
        public string channel;
        public string character;

        public AddChanOP()
        {
            Type = "COA";
        }
    }
}
