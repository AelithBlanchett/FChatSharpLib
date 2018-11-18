using FChatSharpLib.Entities.Plugin;
using FChatSharpLib.Entities.Plugin.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FChatSharp.ExamplePlugin.Commands
{
    class CreateChannel : BaseCommand
    {
        public override string Description => "Creates a Channel";

        public override string ExampleUsage => "!CreateChannel";

        public override BasePlugin MyPlugin { get => OnePluginOneRoom.Plugin; set => OnePluginOneRoom.Plugin = value; }

        public override void ExecuteCommand(string[] args, string channel)
        {
            MyPlugin.FChatClient.CreateChannel("My super channel");
        }

        public CreateChannel()
        {
            
        }
    }
}
