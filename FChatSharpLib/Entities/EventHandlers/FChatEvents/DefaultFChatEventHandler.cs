using FChatSharpLib.Entities.Events;
using FChatSharpLib.Entities.Events.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.EventHandlers.FChatEvents
{
    public static class DefaultFChatEventHandler
    {
        public static Dictionary<string, Type> KnownEvents;

        public static void DetectEvent(string data)
        {
            dynamic detectedEvent = null;
            Type detectedType = null;
            var splittedData = data.Split(new char[] { ' ' }, 2);

            if(KnownEvents == null)
            {
                KnownEvents = new Dictionary<string, Type>();
                KnownEvents = GetAllSupportedEvents();
            }

            if (splittedData.Length == 2)
            {
                if (KnownEvents.ContainsKey(splittedData[0]))
                {
                    detectedType = KnownEvents[splittedData[0]];
                    detectedEvent = JsonConvert.DeserializeObject(splittedData[1], detectedType);
                }
            }

            if(detectedEvent != null)
            {
                ReceivedFChatEvent?.Invoke(null, new ReceivedEventEventArgs()
                {
                    Event = detectedEvent
                });

                if(detectedType == typeof(Message))
                {
                    var castedEvent = (Message)detectedEvent;
                    if (castedEvent != null && !string.IsNullOrWhiteSpace(castedEvent.message) && castedEvent.message.StartsWith("!") && castedEvent.message.Remove(0,1).Length > 1)
                    {
                        var splittedMessage = castedEvent.message.Split(new char[] { ' ' }, 2);
                        var arguments = new string[splittedMessage.Length - 1];

                        if(splittedMessage.Length > 2)
                        {
                            for (int i = 1; i < splittedMessage.Length; i++)
                            {
                                arguments[i - 1] = splittedMessage[i];
                            }
                        }

                        var command = splittedMessage[0];

                        ReceivedPluginCommandEvent?.Invoke(null, new ReceivedPluginCommandEventArgs()
                        {
                            Command = command.Remove(0, 1),
                            Arguments = arguments,
                            Channel = castedEvent.channel,
                            Character = castedEvent.character
                        });
                    }
                }
            }
        }

        public static EventHandler ReceivedFChatEvent;
        public static EventHandler<ReceivedPluginCommandEventArgs> ReceivedPluginCommandEvent;

        private static Type[] GetAllEventTypes()
        {
            var serverEvents = Assembly.GetExecutingAssembly().GetTypes().Where(t => String.Equals(t.Namespace, "FChatLib.Entities.Events.Server", StringComparison.Ordinal)).ToArray();
            return serverEvents;
        }

        private static Dictionary<string, Type> GetAllSupportedEvents()
        {
            var listOfSupportedEvents = new Dictionary<string, Type>();
            var commandTypes = GetAllEventTypes();
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
