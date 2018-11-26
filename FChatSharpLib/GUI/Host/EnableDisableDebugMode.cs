using EasyConsole;
using FChatSharpLib.Entities.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FChatSharpLib.GUI.Host
{
    class EnableDisableDebugMode : Page
    {
        public EnableDisableDebugMode(FChatSharpHost program)
            : base("Enable/Disable debug mode", program)
        {

        }

        public override void Display()
        {
            base.Display();

            FChatSharpHost program = (FChatSharpHost)Program;

            var input = Input.ReadInt("Press 1 to Enable debug mode or 2 if you want to disable it:", 1, 2);
            program.Bot.Events.Debug = (input == 1);

            Output.WriteLine("Debug mode is now set to: " + program.Bot.Events.Debug.ToString());

            Input.ReadString("Press [Enter] to navigate home");
            Program.NavigateHome();
        }
    }
}
