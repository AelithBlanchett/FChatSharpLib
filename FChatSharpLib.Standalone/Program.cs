using FChatSharpLib;
using System;

namespace FChatSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var channel = "ADH-0af075f5654a454a6ba5";
            var bot = new Bot("dollydolly", "", "myrandom", "myrandom", false, 4000);
            bot.Connect();
            //bot.Plugins.LoadPlugin("test", "ADH-55bc847c492faf154406");
            Console.ReadLine();
            //bot.Plugins.ReloadPluginGlobal("test");
            bot.JoinChannel(channel);
            //bot.SendMessage("test", channel);
            Console.ReadLine();
            //bot.Plugins.UnloadPlugin("test");
            //Console.ReadLine();
            //bot.Plugins.UpdateAllPlugins();
            //Console.ReadLine();
            //bot.Plugins.ReloadPluginInChannel("test", "ADH-92a9bd86405869c8a768");
            //Console.ReadLine();
        }
    }
}
