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

        public override BasePlugin MyPlugin { get => OnePluginOneRoom.Plugin; set => OnePluginOneRoom.Plugin = value; }

        public override void ExecuteCommand()
        {
            MyPlugin.FChatClient.SendMessage("Hello everyone! Bye!", MyPlugin.FChatClient.State.Channels.First());
            MyPlugin.FChatClient.SendMessage($"Here are the current present members: {String.Join(", ", MyPlugin.FChatClient.State.ChannelsInfo.First().CharactersInfo.ToList().Select(x => x.Character))}", MyPlugin.FChatClient.State.Channels.First());
        }

        public SayHello()
        {

        }
    }
}
