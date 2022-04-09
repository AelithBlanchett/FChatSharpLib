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
        private readonly string hostname;

        public double FloodLimit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Debug { get; set; }

        public RemoteEvents(string hostname = "localhost", bool debug = false)
        {
            this.hostname = hostname;
            Debug = debug;
        }

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

        public event EventHandler<ReceivedPluginRawDataEventArgs> ReceivedPluginRawData;

        public void StartListening()
        {
            RemoteFChatEventHandler.Connect(hostname);
        }

        public void StopListening()
        {
            RemoteFChatEventHandler.Disconnect();
        }

        public void SendCommand(string commandJson)
        {
            if (Debug)
            {
                Console.WriteLine("REMOTE SENT TO HOST: " + commandJson);
            }
            RemoteFChatEventHandler.SendJsonCommand(commandJson);
        }

        public void SetFloodLimit(double floodLimit)
        {
            throw new NotImplementedException();
        }
    }
}
