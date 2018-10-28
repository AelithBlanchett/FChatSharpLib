using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Client
{
    class KickFromChannel : BaseEvent
    {
        public string channel;
        public string character;

        public KickFromChannel()
        {
            Type = "CKU";
        }
    }
}
