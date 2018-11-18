using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.Entities.Events.Server
{
    public class VariableReceived : BaseFChatEvent
    {
        public string variable;
        public string value;

        public VariableReceived()
        {
            this.Type = "VAR";
        }
    }
}
