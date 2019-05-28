using FChatSharpLib.Entities.Plugin;
using FChatSharpLib.Entities.Plugin.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FChatSharp.ExamplePlugin.Commands
{
    class SayHello : BaseCommand<ExamplePlugin> 
    {
        public override string Description => "Says hello to the room.";

        public override void ExecuteCommand(string character, IEnumerable<string> args, string channel)
        {
            Plugin.FChatClient.SendMessageInChannel("Hello everyone! Bye!", channel);
            var channelInformations = Plugin.FChatClient.State.ChannelsInfo.GetValueOrDefault(channel);
            var informationOfCharactersInChannel = channelInformations.CharactersInfo.ToList();
            var charactersPresentInChannel = informationOfCharactersInChannel.Select(x => x.Character);
            Plugin.FChatClient.SendMessageInChannel($"Here are the current present members: {String.Join(", ", charactersPresentInChannel)}", channel);
            var statusFromChannelListings = informationOfCharactersInChannel.FirstOrDefault(x => x.Character.ToLower() == "aelith blanchette")?.Status;
            var statusFromCharacterListings = Plugin.FChatClient.State.CharactersInfos.GetValueOrDefault("aelith blanchette")?.Status;
            Plugin.FChatClient.SendMessageInChannel($"Here are two methods to access a character's information:", channel);
            Plugin.FChatClient.SendMessageInChannel($"Status 1: {statusFromChannelListings}", channel);
            Plugin.FChatClient.SendMessageInChannel($"Status 2: {statusFromCharacterListings}", channel);
            Plugin.FChatClient.SetStatus(FChatSharpLib.Entities.Events.Helpers.StatusEnum.Busy, "Busy!");
            Plugin.FChatClient.RollDice("1d33", channel);
        }
    }
}
