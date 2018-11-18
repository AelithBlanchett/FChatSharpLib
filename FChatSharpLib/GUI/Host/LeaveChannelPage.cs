using EasyConsole;
using FChatSharpLib.Entities.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FChatSharpLib.GUI.Host
{
    class LeaveChannelPage : Page
    {
        public LeaveChannelPage(BotHoster program)
            : base("Leave a channel", program)
        {
            
        }

        public override void Display()
        {
            base.Display();

            BotHoster program = (BotHoster)Program;

            var channels = program.Bot.State.Channels.ToList();

            for (int i = 0; i < channels.Count; i++)
            {
                Output.WriteLine($"#{i+1}: {channels[i]}");
            }
            var input = Input.ReadInt("Please enter the channel # to leave:", 1, channels.Count);
            Output.WriteLine($"Leaving channel {channels[input-1]}...");

            program.Bot.LeaveChannel(channels[input-1]);

            Input.ReadString("Press [Enter] to navigate home");
            Program.NavigateHome();
        }
    }
}
