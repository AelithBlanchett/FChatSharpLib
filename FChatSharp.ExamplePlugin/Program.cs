using FChatSharpLib;
using FChatSharpLib.Entities.Plugin;
using FChatSharpLib.Plugin;
using System;
using System.Collections.Generic;

namespace FChatSharp.ExamplePlugin
{
    class Program
    {

        public static BasePlugin Plugin { get; set; }

        static void Main(string[] args)
        {
            Plugin = new ExamplePlugin();
        }
    }
}
