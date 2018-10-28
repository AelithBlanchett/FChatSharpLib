using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Client
{
    class SetStatus : BaseEvent
    {
        public string status;
        public string statusmsg;

        public SetStatus()
        {
            Type = "STA";
        }
    }
}
