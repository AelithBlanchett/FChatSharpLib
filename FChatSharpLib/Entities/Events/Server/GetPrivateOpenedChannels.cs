using FChatSharpLib.Entities.Events.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Server
{
    public class GetPrivateOpenedChannels : BaseFChatEvent
    {
        public IEnumerable<FListChannelState> channels;
        public GetPrivateOpenedChannels()
        {
            Type = "ORS";
        }
    }
}
