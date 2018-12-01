using EasyConsole;
using FChatSharpLib.Entities.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FChatSharpLib.GUI.Plugins
{
    class ExecuteCommandPage : Page
    {
        public ExecuteCommandPage(BasePlugin program)
            : base("Execute a chat command", program)
        {
            
        }

        public override void Display()
        {
            base.Display();

            BasePlugin program = (BasePlugin)Program;

            var commands = program.GetCommandList();

            for (int i = 0; i < commands.Count; i++)
            {
                Output.WriteLine($"#{i+1}: {commands[i]}");
            }
            var input = Input.ReadInt("Please enter the command # to execute:", 1, commands.Count);
            var command = commands[input - 1];

            var inputCharacterName = Input.ReadString($"Please enter the character name to use to execute the command (Press Enter for \"{program.FChatClient.State.BotCharacterName}\"):");
            if (string.IsNullOrWhiteSpace(inputCharacterName))
            {
                inputCharacterName = program.FChatClient.State.BotCharacterName;
            }

            var inputArguments = Input.ReadString($"Please enter the arguments to use to execute the command:");
            var arguments = new List<string>();
            if (!string.IsNullOrWhiteSpace(inputArguments))
            {
                arguments = inputArguments.Replace(" ", ",").Split(",").ToList();
            }

            Output.WriteLine($"Executing command {command} with character {inputCharacterName} and arguments {string.Join(",", arguments)}...");

            program.ExecuteCommand(inputCharacterName, command, arguments, program.Channel);

            Input.ReadString("Press [Enter] to navigate home");
            Program.NavigateHome();
        }
    }
}
