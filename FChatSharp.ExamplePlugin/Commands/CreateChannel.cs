using FChatSharpLib.Entities.Plugin;
using FChatSharpLib.Entities.Plugin.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharp.ExamplePlugin.Commands
{
    public class CreateChannel : BaseCommand<ExamplePlugin> 
    {
        public override string Description => "Creates a new room.";

        string newChannelId = "";

        public override async Task ExecuteCommand(string character, IEnumerable<string> args, string channel)
        {
            Plugin.FChatClient.BotCreatedChannel += FChatClient_BotCreatedChannel;
            Plugin.FChatClient.CreateChannel(character + "'s test channel");

            var delay = 100; //in ms
            var totalDelay = 0;

            for (int i = 0; i < 100;  i++)
            {
                if (!string.IsNullOrEmpty(newChannelId))
                {
                    break;
                }
                await Task.Delay(delay);
                totalDelay += delay;
            }

            if (string.IsNullOrEmpty(newChannelId))
            {
                //Error
                Plugin.FChatClient.SendMessageInChannel("There was an error creating the channel.", channel);
                return;
            }

            Plugin.FChatClient.SendMessageInChannel($"New channel created after {totalDelay}ms! It's ID is: {newChannelId}", channel);
        }

        private void FChatClient_BotCreatedChannel(object sender, FChatSharpLib.Entities.Events.Server.JoinChannel e)
        {
            newChannelId = e.channel;
        }
    }
}
