using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Client
{
    public class GetPrivateOpenedChannels : BaseFChatEvent
    {

        public GetPrivateOpenedChannels()
        {
            this.Type = "ORS";
        }
    }
}
