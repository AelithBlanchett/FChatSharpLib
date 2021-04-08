using EasyConsole;
using FChatSharpLib.Entities.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FChatSharpLib.GUI.Plugins
{
    class SendMessagePage : Page
    {
        public SendMessagePage(BasePlugin program)
            : base("Send a message", program)
        {
            
        }

        public static string LastCallingCharacter = "";
        public static string LastArguments = "";

        public override void Display()
        {
            base.Display();

            BasePlugin program = (BasePlugin)Program;

            var channels = program.Channels;
            var channel = program.Channel;

            if(channels.Count > 1)
            {
                for (int i = 0; i < channels.Count; i++)
                {
                    Output.WriteLine($"#{i + 1}: {channels[i]}");
                }
                var input = Input.ReadInt("Please enter the channel # to write in:", 1, channels.Count);

                channel = channels[input - 1];
            }

            var message = Input.ReadString($"Please enter the message you would like to send:");

            Output.WriteLine($"Sending message \"{message}\" in channel {channel}");

            program.FChatClient.SendMessageInChannel(message, channel);

            Input.ReadString("Press [Enter] to navigate home");
            Program.NavigateHome();
        }
    }
}
