using FChatSharpLib.Entities.Plugin;
using FChatSharpLib.Entities.Plugin.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FChatSharp.ExamplePlugin.Commands
{
    class SayHello : BaseCommand
    {
        public override string Description => "Says hello to the room.";

        public override string ExampleUsage => "!hello";

        public override BasePlugin MyPlugin { get => Program.Plugin; set => Program.Plugin = value; }

        public override void ExecuteCommand()
        {
            MyPlugin.FChatClient.SendMessage("Hello everyone! Bye!", MyPlugin.FChatClient.Channels.First());
            MyPlugin.FChatClient.SendMessage("Hello everyone! Bye!", MyPlugin.FChatClient.Channels.First());
            MyPlugin.FChatClient.SendMessage("Hello everyone! Bye!", MyPlugin.FChatClient.Channels.First());
            MyPlugin.FChatClient.SendMessage("Hello everyone! Bye!", MyPlugin.FChatClient.Channels.First());
            MyPlugin.FChatClient.SendMessage("Hello everyone! Bye!", MyPlugin.FChatClient.Channels.First());
            MyPlugin.FChatClient.SendMessage("Hello everyone! Bye!", MyPlugin.FChatClient.Channels.First());
            MyPlugin.FChatClient.SendMessage("Hello everyone! Bye!", MyPlugin.FChatClient.Channels.First());
            //MyPlugin.FChatClient.KickUser("Lance Starr", MyPlugin.FChatClient.Channels.First());
        }

        public SayHello()
        {

        }
    }
}
