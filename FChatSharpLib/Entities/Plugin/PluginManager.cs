using FChatSharpLib.Entities.EventHandlers;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FChatSharpLib.Entities.Plugin
{
    public class PluginManager : MarshalByRefObject
    {

        public List<PluginSpawner> PluginSpawnersList;

        //             key=channel     values=pluginName,pluginClass
        public Dictionary<string, Dictionary<string, BasePlugin>> LoadedPlugins;

        private IModel _pubsubChannel;

        public PluginManager()
        {
            LoadedPlugins = new Dictionary<string, Dictionary<string, BasePlugin>>();
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            _pubsubChannel = connection.CreateModel();
            _pubsubChannel.QueueDeclare(queue: "FChatLib.Plugins",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
            //TODO FIX THIS?!??!?! LoadAllAvailablePlugins();
        }

        public void PassCommandToLoadedPlugins(object sender, ReceivedPluginCommandEventArgs e)
        {
            string serializedCommand = JsonConvert.SerializeObject(e);
            var body = Encoding.UTF8.GetBytes(serializedCommand);
            _pubsubChannel.BasicPublish(exchange: "",
                                 routingKey: "FChatLib.Plugins.ToPlugins",
                                 basicProperties: null,
                                 body: body);

            Console.WriteLine(" PluginManager Sent {0}", serializedCommand);
        }

        //public void LoadAllAvailablePlugins()
        //{
        //    PluginSpawnersList = new List<PluginSpawner>();
        //    var availablePlugins = GetAvailablePlugins();

        //    foreach (var pluginName in availablePlugins)
        //    {
        //        PluginSpawnersList.AddRange(LoadPluginsFromAssembly(pluginName.ToLower()));
        //    }
        //}

        //public void CreatePluginManagerForChannelIfNotExistent(string channel)
        //{
        //    if (!LoadedPlugins.ContainsKey(channel.ToLower()))
        //    {
        //        LoadedPlugins.Add(channel.ToLower(), new Dictionary<string, BasePlugin>());
        //    }
        //}

        //public bool LoadPlugin(string pluginName, string channel)
        //{
        //    var myPlugin = GetPluginInstance(pluginName.ToLower(), channel);
        //    var flag = false;

        //    if (myPlugin != null)
        //    {
        //        CreatePluginManagerForChannelIfNotExistent(channel.ToLower());

        //        if (LoadedPlugins[channel.ToLower()].ContainsKey(pluginName.ToLower()))
        //        {
        //            LoadedPlugins[channel.ToLower()][pluginName.ToLower()] = myPlugin;
        //            flag = true;
        //        }
        //        else
        //        {
        //            LoadedPlugins[channel.ToLower()].Add(pluginName.ToLower(), myPlugin);
        //            flag = true;
        //        }
        //    }

        //    return flag;
        //}

        //public bool UpdatePlugin(string pluginName)
        //{
        //    bool success = true;
        //    try
        //    {
        //        PluginSpawnersList.RemoveAll(x => x.PluginName.ToLower() == pluginName.ToLower());
        //        LoadPluginsFromAssembly(pluginName);
        //    }
        //    catch (Exception)
        //    {
        //        success = false;
        //    }

        //    return success;
        //}

        //public bool UpdateAllPlugins()
        //{
        //    bool success = true;
        //    try
        //    {
        //        PluginSpawnersList.Clear();
        //        LoadAllAvailablePlugins();
        //    }
        //    catch (Exception)
        //    {
        //        success = false;
        //    }

        //    return success;
        //}

        //public bool ReloadPluginGlobal(string pluginName)
        //{
        //    var flag = false;


        //    foreach (var channel in LoadedPlugins.Keys)
        //    {
        //        var myPlugin = GetPluginInstance(pluginName.ToLower(), channel);

        //        if (myPlugin != null)
        //        {
        //            if (LoadedPlugins[channel].ContainsKey(pluginName.ToLower()))
        //            {
        //                LoadedPlugins[channel][pluginName.ToLower()].OnPluginUnload();
        //                LoadedPlugins[channel][pluginName.ToLower()] = null;
        //                LoadedPlugins[channel][pluginName.ToLower()] = myPlugin;
        //                flag = true;
        //            }
        //            else
        //            {
        //                LoadedPlugins[channel].Add(pluginName.ToLower(), myPlugin);
        //                flag = true;
        //            }
        //        }
        //    }


        //    return flag;
        //}

        //public bool ReloadPluginInChannel(string pluginName, string channel)
        //{
        //    var myPlugin = GetPluginInstance(pluginName.ToLower(), channel);

        //    if (myPlugin != null)
        //    {
        //        CreatePluginManagerForChannelIfNotExistent(channel.ToLower());
        //        LoadedPlugins[channel.ToLower()].Add(pluginName.ToLower(), myPlugin);
        //        return true;
        //    }

        //    return false;
        //}

        //public BasePlugin GetPluginInstance(string pluginName, string channel)
        //{
        //    foreach (var pluginSpawner in PluginSpawnersList)
        //    {
        //        if (pluginSpawner.PluginName.ToLower() == pluginName.ToLower())
        //        {
        //            var loadedPlugin = (BasePlugin)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(pluginSpawner.AssemblyName, pluginSpawner.TypeName, false, BindingFlags.Default, null, new object[] { }, System.Globalization.CultureInfo.CurrentCulture, null);
        //            loadedPlugin.OnPluginLoad(channel);
        //            return loadedPlugin;
        //        }
        //    }

        //    return null;
        //}

        //public List<string> GetAvailablePlugins()
        //{
        //    var unformattedPluginsList = System.IO.Directory.EnumerateFiles(Environment.CurrentDirectory, "FChatLib.Plugin.*.dll");
        //    var formattedPluginsList = unformattedPluginsList.ToList();
        //    formattedPluginsList = formattedPluginsList.Select(x => x.Replace($"{Environment.CurrentDirectory}\\", "").Replace("FChatLib.Plugin.", "").Replace(".dll", "").ToLower()).ToList();
        //    return formattedPluginsList;
        //}

        //public void InstallNuGetPlugins(string pluginName)
        //{
        //    var repo = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");

        //    var packageManager = new PackageManager(repo, Environment.CurrentDirectory);
        //    //packageManager.PackageInstalled += PackageManager_PackageInstalled;


        //    var package = repo.FindPackage(pluginName);
        //    if (package != null)
        //    {
        //        packageManager.InstallPackage(package, false, true);
        //    }
        //}

        //public void UpdateNuGetPlugins(IEnumerable<NuGet.IPackageName> pluginNames)
        //{
        //    var repo = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");

        //    var packageManager = new PackageManager(repo, Environment.CurrentDirectory);
        //    //packageManager.PackageInstalled += PackageManager_PackageInstalled;


        //    var packagesToUpdate = repo.GetUpdates(pluginNames, true, true);
        //    if (packagesToUpdate.Any())
        //    {
        //        foreach (var package in packagesToUpdate)
        //        {
        //            Console.WriteLine($"Updating package {package.GetFullName()} to version {package.Version}");
        //            packageManager.UpdatePackage(package, true, true);
        //        }
        //    }
        //}

        //public List<PluginSpawner> LoadPluginsFromAssembly(string pluginName)
        //{
        //    List<PluginSpawner> loadedPlugins = new List<PluginSpawner>();
        //    var wrapper = new PluginSpawner();
        //    try
        //    {
        //        AppDomainSetup domaininfo = new AppDomainSetup()
        //        {
        //            ApplicationBase = Environment.CurrentDirectory,
        //            ShadowCopyDirectories = Environment.CurrentDirectory,
        //            ShadowCopyFiles = "true"
        //        };
        //        Evidence adevidence = AppDomain.CurrentDomain.Evidence;
        //        AppDomain domain = AppDomain.CreateDomain($"AD-{pluginName}", adevidence, domaininfo);

        //        Type type = typeof(TypeProxy);
        //        var value = (TypeProxy)domain.CreateInstanceAndUnwrap(
        //            type.Assembly.FullName,
        //            type.FullName);


        //        Assembly assembly = value.GetAssembly($"{System.Environment.CurrentDirectory}\\FChatLib.Plugin.{pluginName}.dll");
        //        AppDomain.Unload(domain);

        //        foreach (var typ in assembly.GetTypes())
        //        {

        //            if (typeof(IPlugin).IsAssignableFrom(typ))
        //            {
        //                var methodInfos = typ.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        //                //var loadedPlugin = domain.CreateInstanceAndUnwrap(typ.Assembly.FullName, typ.FullName, false, BindingFlags.Default, null, new object[] { null, "" }, System.Globalization.CultureInfo.CurrentCulture, null);
        //                if (methodInfos.FirstOrDefault(x => x.Name == "OnPluginLoad") != null && methodInfos.FirstOrDefault(x => x.Name == "OnPluginLoad") != null)
        //                {
        //                    var pluginWrapper = new PluginSpawner()
        //                    {
        //                        Assembly = assembly,
        //                        Domain = domain,
        //                        AssemblyName = typ.Assembly.FullName,
        //                        TypeName = typ.FullName,
        //                        PluginFileName = $"FChatLib.Plugin.{ pluginName }.dll",
        //                        PluginName = pluginName.ToLower(),
        //                        PluginVersion = assembly.GetName().Version
        //                    };
        //                    loadedPlugins.Add(pluginWrapper);
        //                }

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("failed to load plugin {0}", pluginName);
        //        Console.WriteLine(ex.ToString());
        //    }

        //    return loadedPlugins;

        //}
    }
}
