using FChatSharpLib.Entities.EventHandlers;
using FChatSharpLib.Entities.EventHandlers.FChatEvents;
using FChatSharpLib.Entities.Events;
using FChatSharpLib.Entities.Events.Helpers;
using FChatSharpLib.Entities.Plugin;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib
{
    public class RemoteBotController : BaseBot
    {

        public RemoteBotController(string hostname = "localhost", bool debug = false) : base(new RemoteEvents(hostname, debug))
        {
        }

        public override void Connect()
        {
            Events.ReceivedStateUpdate += Events_ReceivedStateUpdate;
            base.Connect();
        }

        private void Events_ReceivedStateUpdate(object sender, ReceivedStateUpdateEventArgs e)
        {
            State = e.State;
        }
    }
}
