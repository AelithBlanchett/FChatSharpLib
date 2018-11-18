using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Client
{
    public class RollDice : BaseFChatEvent
    {
        public string channel;
        public string dice;

        public RollDice()
        {
            this.Type = "RLL";
        }
    }
}
