using FChatSharpLib.Entities.Events.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Server
{
    public class ListConnectedUsers : BaseFChatEvent
    {
        public List<CharacterState> characters;

        public ListConnectedUsers()
        {
            this.Type = "LIS";
        }
    }
}
