using FChatSharpLib.Entities.EventHandlers;
using FChatSharpLib.Entities.EventHandlers.FChatEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib
{
    public class RemoteEvents : IEvents
    {
        public event EventHandler<ReceivedEventEventArgs> ReceivedFChatEvent
        {
            add { RemoteFChatEventHandler.ReceivedFChatEvent += value; }
            remove { RemoteFChatEventHandler.ReceivedFChatEvent -= value; }
        }

        public event EventHandler<ReceivedPluginCommandEventArgs> ReceivedChatCommand
        {
            add { RemoteFChatEventHandler.ReceivedChatCommand += value; }
            remove { RemoteFChatEventHandler.ReceivedChatCommand -= value; }
        }

        public event EventHandler<ReceivedStateUpdateEventArgs> ReceivedStateUpdate
        {
            add { RemoteFChatEventHandler.ReceivedStateUpdate += value; }
            remove { RemoteFChatEventHandler.ReceivedStateUpdate -= value; }
        }

        public void StartListening()
        {
            RemoteFChatEventHandler.Connect();
        }

        public void StopListening()
        {
            RemoteFChatEventHandler.Disconnect();
        }

        public void SendCommand(string commandJson)
        {
            RemoteFChatEventHandler.SendJsonCommand(commandJson);
        }
    }
}
