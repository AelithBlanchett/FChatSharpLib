using FChatSharpLib.Entities.Plugin;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FChatSharp.ExamplePlugin
{
    class ExamplePlugin : BasePlugin
    {
        public override string Name => "ExamplePlugin";

        public override string Version => "0.0.1";

        private Timer _kickMonitor;

        public ExamplePlugin(string channel) : base(channel)
        {
            StartTimers();
        }

        public ExamplePlugin(params string[] channels) : base(channels)
        {
            StartTimers();
        }

        private void StartTimers()
        {
            _kickMonitor = new Timer(AutoKickNonShemales, null, 1000, 5000);
            FChatClient.UserJoinedChannel += FChatClient_UserJoinedChannel;
            FChatClient.BotCreatedChannel += FChatClient_BotCreatedChannel;
        }

        private void FChatClient_UserJoinedChannel(object sender, FChatSharpLib.Entities.Events.Server.JoinChannel e)
        {
            FChatClient.SendMessageInChannel($"Hey, {e.character.identity} just joined us!", e.channel);
        }

        private void FChatClient_BotCreatedChannel(object sender, FChatSharpLib.Entities.Events.Server.InitialChannelData e)
        {
            FChatClient.ChangeChannelPrivacy(e.channel, false);
        }

        private void AutoKickNonShemales(object state)
        {
            if (!FChatClient.IsUserAdmin(FChatClient.State.BotCharacterName, Channel))
            {
                return;
            }
            var characters = FChatClient.State.GetAllCharactersInChannel(Channel);
            foreach (var character in characters)
            {
                if (character.Gender != FChatSharpLib.Entities.Events.Helpers.GenderEnum.Shemale && !FChatClient.IsSelf(character.Character) && !FChatClient.IsUserAdmin(character.Character, Channel))
                {
                    FChatClient.KickUser(character.Character, Channel);
                }
            }
        }
    }
}
