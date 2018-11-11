using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Plugin
{
    public interface IPlugin
    {
        Guid PluginId { get; }
        string Name { get; }
        string Version { get; }
        IBot FChatClient { get; }
        List<string> GetCommandList();
        void OnPluginLoad();
        void OnPluginUnload();
    }
}
