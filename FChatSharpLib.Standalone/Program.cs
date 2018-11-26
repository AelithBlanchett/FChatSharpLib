using FChatSharpLib;
using System;
using System.Threading.Tasks;

namespace FChatSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new FChatSharpHost("username", "password", "BotCharacterName", "ASuperDeveloper", true, 4000);
        }
    }
}
