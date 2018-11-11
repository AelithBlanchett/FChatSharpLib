using FChatSharpLib.Entities.Events.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Events
{
    public abstract class BaseFChatEvent : IBaseFChatEvent
    {
        private string _type;

        [JsonIgnore]
        public string Data
        {
            get
            {
                return JsonConvert.SerializeObject(this);
            }
            set
            {
                
                dynamic baseEventSerialized = JsonConvert.DeserializeObject(value);
                var fieldList = this.GetType().UnderlyingSystemType.GetFields();
                foreach (var prop in fieldList)
                {
                    var receivedValue = baseEventSerialized[prop.Name];
                    if (receivedValue != null)
                    {
                        prop.SetValue(this, receivedValue.ToString());
                    }
                }
            }
        }

        [JsonIgnore]
        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public override string ToString()
        {
            return $"{Type} {Data}";
        }

        public static object Deserialize(string content)
        {
            var contentType = content.Split(new char[] { ' ' }, 2);
            object returnedCommand = null;
            switch (contentType[0].Trim())
            {
                case "MSG":
                    returnedCommand = new Message()
                    {
                        Type = "MSG",
                        Data = contentType[1].Trim()
                    };
                    break;
                default:
                    break;
            }

            return returnedCommand;
        }
    }
}
