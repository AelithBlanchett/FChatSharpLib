using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Server
{
    public class OfflineNotification : BaseFChatEvent
    {
        public string character;

        public OfflineNotification()
        {
            this.Type = "FLN";
        }
    }
}
