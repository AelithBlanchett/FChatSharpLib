using FChatSharpLib.Entities.Events.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Server
{
    public class JoinChannel : BaseFChatEvent
    {
        public CharacterInfo character;
        public string title;
        public string channel;

        public JoinChannel()
        {
            this.Type = "JCH";
        }
    }
}
