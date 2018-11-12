using FChatSharpLib;
using FChatSharpLib.Entities.Plugin;
using FChatSharpLib.Plugin;
using System;
using System.Collections.Generic;

namespace FChatSharp.ExamplePlugin
{
    class OnePluginOneRoom
    {

        public static BasePlugin Plugin { get; set; }

        public static void Main(string[] args)
        {
            Plugin = new ExamplePlugin("adh-fb2ea279def7c1d17022");
        }
    }
}
