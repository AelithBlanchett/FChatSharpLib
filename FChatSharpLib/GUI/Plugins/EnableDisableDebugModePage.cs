using EasyConsole;
using FChatSharpLib.Entities.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FChatSharpLib.GUI.Host
{
    class EnableDisableDebugModePage : Page
    {
        public EnableDisableDebugModePage(BasePlugin program)
            : base("Enable/Disable debug mode", program)
        {

        }

        public override void Display()
        {
            base.Display();

            BasePlugin program = (BasePlugin)Program;

            var input = Input.ReadInt("Press 1 to Enable debug mode or 2 if you want to disable it:", 1, 2);
            program.FChatClient.Events.Debug = (input == 1);

            Output.WriteLine("Debug mode is now set to: " + program.FChatClient.Events.Debug.ToString());

            Input.ReadString("Press [Enter] to navigate home");
            Program.NavigateHome();
        }
    }
}
