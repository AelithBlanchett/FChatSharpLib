using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Server
{
    public class OnlineNotification : BaseFChatEvent
    {
        public string identity;
        public string gender;
        public string status;

        public OnlineNotification()
        {
            this.Type = "NLN";
        }
    }
}
