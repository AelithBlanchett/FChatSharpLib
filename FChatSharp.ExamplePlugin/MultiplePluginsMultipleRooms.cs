using FChatSharpLib;
using FChatSharpLib.Entities.Plugin;
using System;
using System.Collections.Generic;

namespace FChatSharp.ExamplePlugin
{
    class MultiplePluginsMultipleRooms
    {

        public static Dictionary<string, BasePlugin> Plugins { get; set; }

        public static void Main(string[] args)
        {
            var channelsList = new List<string>(){ "adh-a0d618f7c2e36f8959ea", "adh-a0d618f7c2e36f8959ea" };
            foreach (var channel in channelsList)
            {
                Plugins.Add(channel, new ExamplePlugin(channel));
            }
        }
    }
}
