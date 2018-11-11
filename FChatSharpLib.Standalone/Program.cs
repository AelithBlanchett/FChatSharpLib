using FChatSharpLib;
using System;
using System.Threading.Tasks;

namespace FChatSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var channel = "ADH-a0d618f7c2e36f8959ea";
            var bot = new Bot("dollydolly", "dollydolly00", "myrandom", "myrandom", false, 4000);
            bot.Connect();
            //bot.Plugins.LoadPlugin("test", "ADH-55bc847c492faf154406");
            while (!bot.IsBotReady)
            {
                Task.Delay(1000).ConfigureAwait(false);
            }
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
