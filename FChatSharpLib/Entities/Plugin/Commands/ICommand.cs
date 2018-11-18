﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Plugin.Commands
{
    public interface ICommand
    {
        string Description { get; }
        string ExampleUsage { get; }
        void ExecuteCommand(string[] args, string channel);
        BasePlugin MyPlugin { get; set; }
    }
}
