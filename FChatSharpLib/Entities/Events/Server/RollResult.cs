using FChatSharpLib.Entities.Events.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Server
{
    public class RollResult : BaseFChatEvent
    {        
        public string channel;
        public IEnumerable<int> results;
        public string type;
        public string message;
        public IEnumerable<string> rolls;
        public string character;
        public string endresult;

        public string target;

        public RollResult()
        {
            Type = "RLL";
        }
    }
}
