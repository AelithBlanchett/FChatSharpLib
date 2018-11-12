using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Client
{
    public class JoinChannel : BaseFChatEvent
    {
        public string channel;

        public JoinChannel()
        {
            this.Type = "JCH";
        }
    }
}
