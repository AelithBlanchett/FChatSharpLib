﻿using FChatSharpLib.Entities.Plugin;

namespace FChatSharp.ExamplePlugin
{
    class OnePluginOneRoom
    {

        public static BasePlugin Plugin { get; set; }

        public static void Main(string[] args)
        {
            Plugin = new ExamplePlugin("ADH-4358b65b80c8f9920288");
        }
    }
}
