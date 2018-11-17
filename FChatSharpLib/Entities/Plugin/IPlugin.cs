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
        string Channel { get; }
        List<string> Channels { get; set; }
        List<string> GetCommandList();
        void OnPluginLoad();
        void OnPluginUnload();
        bool SingleChannelPlugin { get; set; }
        void AddHandledChannel(string channel);
        void RemoveHandledChannel(string channel);
    }
}
