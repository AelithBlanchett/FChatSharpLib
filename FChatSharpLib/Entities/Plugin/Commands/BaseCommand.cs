﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Plugin.Commands
{
    public abstract class BaseCommand<TPlugin> where TPlugin : BasePlugin, new()
    {
        public virtual string Description { get; }
        public virtual string ExampleUsage { get; }
        public TPlugin Plugin { get; set; }
        public abstract void ExecuteCommand(string characterCalling, string[] args, string channel);
    }
}
