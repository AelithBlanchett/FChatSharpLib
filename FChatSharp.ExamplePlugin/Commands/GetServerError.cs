using FChatSharpLib.Entities.Plugin;
using FChatSharpLib.Entities.Plugin.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharp.ExamplePlugin.Commands
{
    public class GetServerError : BaseCommand<ExamplePlugin> 
    {
        public override string Description => "Says hello to the room.";

        public override async Task ExecuteCommand(string character, IEnumerable<string> args, string channel)
        {
            Plugin.FChatClient.SendCommandToServer("ZZZ {\"channel\":\"test\"}");
        }
    }
}
