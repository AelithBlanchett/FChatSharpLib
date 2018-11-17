using FChatSharpLib.Entities.Plugin;

namespace FChatSharp.ExamplePlugin
{
    class OnePluginOneRoom
    {

        public static BasePlugin Plugin { get; set; }

        public static void Main(string[] args)
        {
            Plugin = new ExamplePlugin("ADH-40b2e092cd976d501afd");
        }
    }
}
