using FChatSharpLib.Entities.EventHandlers;
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

        public static void HandleSpecialEvents(string data, EventHandler<ReceivedEventEventArgs> ReceivedFChatEventHandler, EventHandler<ReceivedPluginCommandEventArgs> ReceivedChatCommandHandler)
        {
            dynamic detectedEvent = null;

            detectedEvent = GetParsedEvent(data, true);

            if (detectedEvent != null)
            {
                ReceivedFChatEventHandler?.Invoke(null, new ReceivedEventEventArgs()
                {
                    Event = detectedEvent
                });

                if (detectedEvent.GetType() == typeof(Message) || detectedEvent.GetType() == typeof(PrivateMessage))
                {
                    var messageText = "";
                    var channel = "";
                    var character = "";

                    if (detectedEvent.GetType() == typeof(Message))
                    {
                        var castedEvent = (Message)detectedEvent;
                        if (castedEvent == null) { return; }
                        messageText = castedEvent.message;
                        channel = castedEvent.channel;
                        character = castedEvent.character;
                    }
                    else if (detectedEvent.GetType() == typeof(PrivateMessage))
                    {
                        var castedEvent = (PrivateMessage)detectedEvent;
                        if (castedEvent == null) { return; }
                        messageText = castedEvent.message;
                        channel = "";
                        character = castedEvent.character;
                    }

                    if (!string.IsNullOrWhiteSpace(messageText) && messageText.StartsWith("!") && messageText.Remove(0, 1).Length > 1)
                    {
                        var splittedMessage = messageText.Split(new char[] { ' ' }, 2);
                        var arguments = new List<string>();
                        if(splittedMessage.Count() > 1)
                        {
                            arguments.AddRange(splittedMessage[1].Replace(" ", "+-/*").Split("+-/*"));
                        }
                        

                        var command = splittedMessage[0];

                        ReceivedChatCommandHandler?.Invoke(null, new ReceivedPluginCommandEventArgs()
                        {
                            Command = command.Remove(0, 1),
                            Arguments = arguments.ToArray(),
                            Channel = channel,
                            Character = character
                        });
                    }
                }
            }
        }

        public static BaseFChatEvent GetParsedEvent(string data, bool serverEntity)
        {
            dynamic detectedEvent = null;
            Type detectedType = null;
            var splittedData = data.Split(new char[] { ' ' }, 2);

            KnownEvents = new Dictionary<string, Type>();
            KnownEvents = GetAllSupportedEvents(serverEntity);

            if (KnownEvents.ContainsKey(splittedData[0]))
            {
                detectedType = KnownEvents[splittedData[0]];
                if (detectedType.Name == nameof(ListConnectedUsers))
                {
                    dynamic baseEntity = JsonConvert.DeserializeObject(splittedData[1]);

                    //We make sure it's not already correctly parsed
                    ListConnectedUsers lisEvent = null;
                    try
                    {
                        lisEvent = JsonConvert.DeserializeObject<ListConnectedUsers>(splittedData[1]);
                    }
                    catch (Exception)
                    {
                    }

                    if (lisEvent == null)
                    {
                        lisEvent = new ListConnectedUsers();
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
                    }

                    detectedEvent = lisEvent;
                }
                else if(splittedData.Length > 1)
                {
                    try
                    {
                        detectedEvent = JsonConvert.DeserializeObject(splittedData[1], detectedType);
                    }
                    catch (Exception ex)
                    {
                        if(splittedData[0] != "VAR")
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }

                }
                else
                {
                    try
                    {
                        detectedEvent = Activator.CreateInstance(detectedType); 
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }

            return detectedType != null ? detectedEvent : null;
        }

        public static T GetEnumEquivalent<T>(string gender) where T : Enum
        {
            var availableValues = Enum.GetNames(typeof(T));
            var realName = availableValues.FirstOrDefault(x => x.ToLower().Replace("_", "-") == gender.ToLower().Replace("_", "-"));
            if(realName == null)
            {
                realName = availableValues.First();
            }
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
