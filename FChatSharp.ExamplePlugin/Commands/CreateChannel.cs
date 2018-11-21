using FChatSharpLib.Entities.Plugin;
using FChatSharpLib.Entities.Plugin.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FChatSharp.ExamplePlugin.Commands
{
    class CreateChannel : BaseCommand<ExamplePlugin>
    {
        public string Description => "Creates a Channel";

        public string ExampleUsage => "!CreateChannel";

        public override void ExecuteCommand(string character, string[] args, string channel)
        {
            Plugin.FChatClient.CreateChannel("My super channel");
        }

        public CreateChannel()
        {
            
        }
    }
}
