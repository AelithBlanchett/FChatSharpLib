using FChatSharpLib;
using FChatSharpLib.Entities.Plugin;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Volo.Abp.DependencyInjection;

namespace FChatSharp.ExamplePlugin
{
    public class ExamplePlugin : BasePlugin, ISingletonDependency
    {
        public Timer KickMonitor { get; set; }

        public ExamplePlugin(IOptions<ExamplePluginOptions> pluginOptions, RemoteBotController fChatClient) : base(pluginOptions, fChatClient)
        {
            StartTimers();
            Run();
        }

        private void StartTimers()
        {
            KickMonitor = new Timer(AutoKickNonShemales, null, 1000, 5000);
            FChatClient.UserJoinedChannel += FChatClient_UserJoinedChannel;
            FChatClient.BotCreatedChannel += FChatClient_BotCreatedChannel;
        }

        private void FChatClient_UserJoinedChannel(object sender, FChatSharpLib.Entities.Events.Server.JoinChannel e)
        {
            FChatClient.SendMessageInChannel($"Hey, {e.character.identity} just joined us!", e.channel);
        }

        private void FChatClient_BotCreatedChannel(object sender, FChatSharpLib.Entities.Events.Server.InitialChannelData e)
        {
            FChatClient.ChangeChannelPrivacy(false, e.channel);
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
