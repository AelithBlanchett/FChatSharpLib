using EasyConsole;
using FChatSharpLib.Entities.Plugin;
using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.GUI.Plugins
{
    class JoinChannelPage : Page
    {
        public JoinChannelPage(BasePlugin program)
            : base("Join a channel", program)
        {
            
        }

        public override void Display()
        {
            base.Display();

            var input = Input.ReadString("Please enter the channel ID to join (ADH-0x0x0xx0x0x0x0x):");
            Output.WriteLine($"Joining channel {input}...");

            input = input.Trim().ToLower();
            BasePlugin program = (BasePlugin)Program;
            program.AddHandledChannel(input);
            program.FChatClient.JoinChannel(input);
            

            Input.ReadString("Press [Enter] to navigate home");
            Program.NavigateHome();
        }
    }
}
