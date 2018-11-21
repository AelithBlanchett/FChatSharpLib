using FChatSharpLib.Entities.Plugin;

namespace FChatSharp.ExamplePlugin
{
    class OnePluginOneRoom
    {

        public static BasePlugin Plugin { get; set; }

        public static void Main(string[] args)
        {
            Plugin = new ExamplePlugin("ADH-aede471e10a05cf1d1ae");
        }
    }
}
