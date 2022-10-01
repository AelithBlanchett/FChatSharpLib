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

        public static string LastCallingCharacter = "";
        public static string LastArguments = "";

        public async override void Display()
        {
            base.Display();

            BasePlugin program = (BasePlugin)Program;

            if (string.IsNullOrWhiteSpace(LastCallingCharacter))
            {
                LastCallingCharacter = program.FChatClient.State.BotCharacterName;
            }

            var channels = program.Channels;
            var channel = "";

            if (channels.Count > 1)
            {
                for (int i = 0; i < channels.Count; i++)
                {
                    Output.WriteLine($"#{i + 1}: {channels[i]}");
                }
                var inputChannel = Input.ReadInt("Please enter the channel # to send the command in:", 1, channels.Count);

                channel = channels[inputChannel - 1];
            }

            var commands = program.GetCommandList();

            for (int i = 0; i < commands.Count; i++)
            {
                Output.WriteLine($"#{i+1}: {commands[i]}");
            }
            var input = Input.ReadInt("Please enter the command # to execute:", 1, commands.Count);
            var command = commands[input - 1];

            var inputCharacterName = Input.ReadString($"Please enter the character name to use to execute the command (Press Enter for \"{LastCallingCharacter}\"):");
            if (string.IsNullOrWhiteSpace(inputCharacterName))
            {
                inputCharacterName = LastCallingCharacter;
            }
            LastCallingCharacter = inputCharacterName;

            var inputArguments = Input.ReadString($"Please enter the arguments to use to execute the command:");
            var arguments = new List<string>();

            if (!string.IsNullOrWhiteSpace(inputArguments))
            {
                arguments = inputArguments.Replace(" ", ",").Split(",").ToList();
            }

            Output.WriteLine($"Executing command {command} with character {inputCharacterName} and arguments {string.Join(",", arguments)}...");

            await program.TryExecuteCommand(inputCharacterName, command, arguments, program.Channel.ToLowerInvariant());

            Input.ReadString("Press [Enter] to navigate home");
            Program.NavigateHome();
        }
    }
}
