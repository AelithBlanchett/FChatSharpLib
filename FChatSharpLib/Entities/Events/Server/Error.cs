using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Server
{
    public class Error : BaseFChatEvent
    {
        public int number;
        public string message;

        public Error()
        {
            this.Type = "ERR";
        }
    }
}
