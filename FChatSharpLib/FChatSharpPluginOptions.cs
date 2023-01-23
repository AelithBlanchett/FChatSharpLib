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

        public FChatSharpPluginOptions(bool debug, List<string> channels)
        {
            Debug = debug;
            Channels = channels;
        }

        public bool Debug  { get; set; }

        public List<string> Channels { get; set; }
    }
}
