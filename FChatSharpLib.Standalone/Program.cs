using FChatSharpLib;
using System;
using System.Threading.Tasks;

namespace FChatSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new Bot("dollydolly", "", "myrandom", "myrandom", false, 4000);
            bot.Connect();
        }
    }
}
