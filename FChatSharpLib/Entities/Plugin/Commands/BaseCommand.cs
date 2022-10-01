using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Plugin.Commands
{
    public abstract class BaseCommand<TPlugin> where TPlugin : BasePlugin
    {
        public virtual string Description { get; }
        public virtual string ExampleUsage { get
            {
                return $"!{GetType().Name}";
            }
        }
        public TPlugin Plugin { get; set; }


        /// <summary>
        /// The main function of the command.
        /// </summary>
        /// <param name="characterCalling">The character calling the command</param>
        /// <param name="args">Arguments passed to the command. To get all arguments in a single line: string.Join(' ', args);</param>
        /// <param name="channel">The channel in which the command has been executed.</param>
        public abstract Task ExecuteCommand(string characterCalling, IEnumerable<string> args, string channel);

        public virtual Task ExecutePrivateCommand(string characterCalling, IEnumerable<string> args)
        {
            Plugin.FChatClient.SendPrivateMessage("You cannot use that command in private.", characterCalling);
            return Task.CompletedTask;
        }
    }
}
