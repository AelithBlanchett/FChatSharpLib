using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FChatSharpLib.Entities.Events
{
    public class FChatEventParser
    {
        public static Dictionary<string, Type> KnownEvents;

        public static BaseFChatEvent GetParsedEvent(string data, bool serverEntity)
        {
            dynamic detectedEvent = null;
            Type detectedType = null;
            var splittedData = data.Split(new char[] { ' ' }, 2);

            KnownEvents = new Dictionary<string, Type>();
            KnownEvents = GetAllSupportedEvents(serverEntity);

            if (splittedData.Length == 2)
            {
                if (KnownEvents.ContainsKey(splittedData[0]))
                {
                    detectedType = KnownEvents[splittedData[0]];
                    detectedEvent = JsonConvert.DeserializeObject(splittedData[1], detectedType);
                }
            }

            return detectedType != null ? detectedEvent : null;
        }

        private static Type[] GetAllEventTypes(bool serverEntities)
        {
            var serverEvents = Assembly.GetExecutingAssembly().GetTypes().Where(t => String.Equals(t.Namespace, $"FChatSharpLib.Entities.Events.{(serverEntities == true ? "Server" : "Client")}", StringComparison.Ordinal)).ToArray();
            return serverEvents;
        }

        private static Dictionary<string, Type> GetAllSupportedEvents(bool serverEntities)
        {
            var listOfSupportedEvents = new Dictionary<string, Type>();
            var commandTypes = GetAllEventTypes(serverEntities);
            foreach (var type in commandTypes)
            {
                dynamic obj = Activator.CreateInstance(type);
                var property = obj.GetType().GetProperty("Type");
                var value = property.GetValue(obj, null);
                if (!string.IsNullOrEmpty(value))
                {
                    listOfSupportedEvents.Add(value, type);
                }
            }
            return listOfSupportedEvents;
        }
    }
}
