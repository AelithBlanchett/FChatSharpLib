using FChatSharpLib.Entities.Events.Helpers;
using FChatSharpLib.Entities.Events.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                    if(detectedType.Name == nameof(ListConnectedUsers)){
                        dynamic baseEntity = JsonConvert.DeserializeObject(splittedData[1]);
                        var lisEvent = new ListConnectedUsers();
                        lisEvent.characters = new List<Helpers.CharacterState>();
                        foreach (var item in baseEntity.characters)
                        {
                            lisEvent.characters.Add(new Helpers.CharacterState()
                            {
                                Character = item[0].ToString(),
                                Gender = GetEnumEquivalent<GenderEnum>(item[1].ToString().ToLower()),
                                Status = GetEnumEquivalent<StatusEnum>(item[2].ToString().ToLower()),
                                StatusText = item[3].ToString()
                            });
                        }
                        detectedEvent = lisEvent;
                    }
                    else
                    {
                        detectedEvent = JsonConvert.DeserializeObject(splittedData[1], detectedType);
                    } 
                }
            }

            return detectedType != null ? detectedEvent : null;
        }

        public static T GetEnumEquivalent<T>(string gender) where T : Enum
        {
            var availableValues = Enum.GetNames(typeof(T));
            var realName = availableValues.First(x => x.ToLower().Replace("_", "-") == gender.ToLower().Replace("_", "-"));
            return (T)Enum.Parse(typeof(T), realName);
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
