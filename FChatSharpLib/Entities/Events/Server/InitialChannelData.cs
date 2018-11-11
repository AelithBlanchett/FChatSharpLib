using FChatSharpLib.Entities.Events.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Server
{
    public class InitialChannelData : BaseFChatEvent
    {
        public IEnumerable<CharacterInfo> users;
        public string channel;
        public string mode;
    }
}
