﻿using FChatSharpLib.Entities.Plugin;
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
            MyPlugin.FChatClient.SendMessage($"Here are the current present members: {String.Join(", ", MyPlugin.FChatClient.State.ChannelsInfo.First().CharactersInfo.ToList().Select(x => x.Character))}", MyPlugin.FChatClient.Channels.First());
            var chanStatus = MyPlugin.FChatClient.State.ChannelsInfo.First().CharactersInfo.ToList().FirstOrDefault(x => x.Character == "Lance Starr");
            var charStatus = MyPlugin.FChatClient.State.CharactersInfos.ToList().FirstOrDefault(x => x.Character == "Lance Starr");
            MyPlugin.FChatClient.SendMessage($"Channel Status: {chanStatus.Character + " " + chanStatus.Status}\nGlobal Status: {charStatus.Character + " " + charStatus.Status}", MyPlugin.FChatClient.Channels.First());
            //MyPlugin.FChatClient.KickUser("Lance Starr", MyPlugin.FChatClient.Channels.First());
        }

        public SayHello()
        {

        }
    }
}
