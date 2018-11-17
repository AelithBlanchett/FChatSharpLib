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
            foreach (var channel in MyPlugin.Channels)
            {
                MyPlugin.FChatClient.SendMessage("Hello everyone! Bye!", channel);
                var channelInformations = MyPlugin.FChatClient.State.ChannelsInfo.GetValueOrDefault(channel);
                var informationOfCharactersInChannel = channelInformations.CharactersInfo.ToList();
                var charactersPresentInChannel = informationOfCharactersInChannel.Select(x => x.Character);
                MyPlugin.FChatClient.SendMessage($"Here are the current present members: {String.Join(", ", charactersPresentInChannel)}", channel);
                var statusFromChannelListings = informationOfCharactersInChannel.FirstOrDefault(x => x.Character.ToLower() == "aelith blanchette")?.Status;
                var statusFromCharacterListings = MyPlugin.FChatClient.State.CharactersInfos.GetValueOrDefault("aelith blanchette")?.Status;
                MyPlugin.FChatClient.SendMessage($"Here are two methods to access a character's information:", channel);
                MyPlugin.FChatClient.SendMessage($"Status 1: {statusFromChannelListings}", channel);
                MyPlugin.FChatClient.SendMessage($"Status 2: {statusFromCharacterListings}", channel);
            }
        }

        public SayHello()
        {

        }
    }
}
