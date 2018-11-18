using EasyConsole;
using FChatSharpLib.Entities.Plugin;
using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.GUI.Plugins
{
    class LeaveChannelPage : Page
    {
        public LeaveChannelPage(BasePlugin program)
            : base("Leave a channel", program)
        {
            
        }

        public override void Display()
        {
            base.Display();

            BasePlugin program = (BasePlugin)Program;

            var channels = program.Channels;

            for (int i = 0; i < channels.Count; i++)
            {
                Output.WriteLine($"#{i+1}: {channels[i]}");
            }
            var input = Input.ReadInt("Please enter the channel # to leave:", 1, channels.Count);
            Output.WriteLine($"Leaving channel {channels[input-1]}...");

            program.RemoveHandledChannel(channels[input-1]);
            program.FChatClient.LeaveChannel(channels[input-1]);

            Input.ReadString("Press [Enter] to navigate home");
            Program.NavigateHome();
        }
    }
}
