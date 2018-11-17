using FChatSharpLib.Entities.Plugin;

namespace FChatSharp.ExamplePlugin
{
    class OnePluginOneRoom
    {

        public static BasePlugin Plugin { get; set; }

        public static void Main(string[] args)
        {
            Plugin = new ExamplePlugin("ADH-fed4de54751b458d2dc5");
        }
    }
}
