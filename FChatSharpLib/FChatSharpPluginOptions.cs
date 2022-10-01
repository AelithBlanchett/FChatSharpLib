using EasyConsole;
using FChatSharpLib.GUI;
using FChatSharpLib.GUI.Host;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FChatSharpLib
{
    public class FChatSharpPluginOptions : IFChatSharpOptions
    {
        public FChatSharpPluginOptions()
        {

        }

        public FChatSharpPluginOptions(bool debug, bool showConsole, List<string> channels)
        {
            Debug = debug;
            ShowConsole = showConsole;
            Channels = channels;
        }

        public bool Debug  { get; set; }

        public bool ShowConsole { get; set; }

        public List<string> Channels { get; set; }
    }
}
