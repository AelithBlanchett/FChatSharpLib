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

        public static void HandleSpecialEvents(string data)
        {
            dynamic detectedEvent = null;

            detectedEvent = FChatEventParser.GetParsedEvent(data, true);

            if (detectedEvent != null)
            {
                ReceivedFChatEvent?.Invoke(null, new ReceivedEventEventArgs()
                {
                    Event = detectedEvent
                });

                if(detectedEvent.GetType() == typeof(Message))
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

                        ReceivedChatCommand?.Invoke(null, new ReceivedPluginCommandEventArgs()
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

        public static EventHandler<ReceivedEventEventArgs> ReceivedFChatEvent;
        public static EventHandler<ReceivedPluginCommandEventArgs> ReceivedChatCommand;


    }
}
