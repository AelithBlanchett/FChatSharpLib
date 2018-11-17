using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Client
{
    public class InviteUserToCreatedChannel : BaseFChatEvent
    {
        public string character;
        public string channel;

        public InviteUserToCreatedChannel()
        {
            this.Type = "CIU";
        }
    }
}
