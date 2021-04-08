using EasyConsole;
using FChatSharpLib.Entities.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FChatSharpLib.GUI.Plugins
{
    class BroadcastMessagePage : Page
    {
        public BroadcastMessagePage(BasePlugin program)
            : base("Broadcast a message", program)
        {
            
        }

        public override void Display()
        {
            base.Display();

            BasePlugin program = (BasePlugin)Program;

            var channels = program.Channels;

            var message = Input.ReadString($"Please enter the message you would like to send:");

            for (int i = 0; i < channels.Count; i++)
            {
                var channel = channels[i];
                Output.WriteLine($"Broadcasting message \"{message}\" to channel {channel}");
                program.FChatClient.SendMessageInChannel(message, channel);
            }

            Input.ReadString("Press [Enter] to navigate home");
            Program.NavigateHome();
        }
    }
}
