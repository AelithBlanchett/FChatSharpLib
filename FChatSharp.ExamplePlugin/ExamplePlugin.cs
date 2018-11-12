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

        public ExamplePlugin()
        {
            _kickMonitor = new Timer(AutoKickNonShemales, null, 1000, 5000);
        }

        private void AutoKickNonShemales(object state)
        {
            
            foreach (var channel in FChatClient.State.Channels)
            {
                if (!FChatClient.IsUserAdmin(FChatClient.State.BotCharacterName, channel))
                {
                    continue;
                }
                var characters = FChatClient.State.GetAllCharactersInChannel(channel);
                foreach (var character in characters)
                {
                    if(character.Gender != FChatSharpLib.Entities.Events.Helpers.GenderEnum.Shemale && !FChatClient.IsSelf(character.Character) && !FChatClient.IsUserAdmin(character.Character, channel))
                    {
                        FChatClient.KickUser(character.Character, channel);
                    }
                }
            }
            
        }
    }
}
