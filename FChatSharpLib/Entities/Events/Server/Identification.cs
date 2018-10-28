using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Server
{
    public class Identification : BaseEvent
    {
        public string character;

        public Identification()
        {
            this.Type = "IDN";
        }
    }
}
