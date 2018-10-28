using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events.Client
{
    [Serializable]
    public class Identification : BaseEvent
    {
        public string method;
        public string account;
        public string ticket;
        public string character;
        [JsonProperty("cname")]
        public string botCreator;
        [JsonProperty("cversion")]
        public string botVersion;

        public Identification()
        {
            this.Type = "IDN";
        }
    }
}
