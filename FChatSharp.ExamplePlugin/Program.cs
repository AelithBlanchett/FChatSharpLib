using FChatSharpLib;
using FChatSharpLib.Entities.Plugin;
using FChatSharpLib.Plugin;
using System;
using System.Collections.Generic;

namespace FChatSharp.ExamplePlugin
{
    class PluginSpawner
    {

        public static BasePlugin Plugin { get; set; }

        static void Main(string[] args)
        {
            Plugin = new ExamplePlugin("adh-a0d618f7c2e36f8959ea");
        }
    }
}
