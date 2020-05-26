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
        public abstract void ExecuteCommand(string characterCalling, IEnumerable<string> args, string channel);

        public void ExecutePrivateCommand(string characterCalling, IEnumerable<string> args)
        {
            Plugin.FChatClient.SendPrivateMessage("You cannot use that command in private.", characterCalling);
        }
    }
}
