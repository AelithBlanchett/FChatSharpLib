using FChatSharpLib;
using System;
using System.Threading.Tasks;

namespace FChatSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new BotHoster("username", "password", "BotCharacterName", "ASuperDeveloper", true, 4000);
        }
    }
}
