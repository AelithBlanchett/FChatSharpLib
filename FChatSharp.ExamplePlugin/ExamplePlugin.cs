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
            _kickMonitor = new Timer(AutoKickNonShemales, null, 1000, 5000);
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
