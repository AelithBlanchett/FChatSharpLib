using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Plugin
{
    public class PluginSpawner
    {
        public Assembly Assembly;
        public AppDomain Domain;
        public string AssemblyName;
        public string TypeName;
        public string PluginName;
        public string PluginFileName;
        public Version PluginVersion;
    }
}
