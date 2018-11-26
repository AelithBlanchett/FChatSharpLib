using EasyConsole;
using FChatSharpLib.Entities.EventHandlers;
using FChatSharpLib.Entities.Plugin.Commands;
using FChatSharpLib.GUI.Plugins;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Plugin
{
    public abstract class BasePlugin : Program, IPlugin
    {
        public BaseBot FChatClient { get; set; }
        public string Channel { get; set; }
        public List<string> Channels { get; set; }
        public string Name { get; set; }
        public string Version { get; set;  }
        public Guid PluginId { get; set; }
        public bool IsInDebug { get; set; }
        public bool SingleChannelPlugin
        {
            get
            {
                return Channels.Count == 1;
            }
        }
        public int ChannelsConnectedTo
        {
            get
            {
                return Channels.Count();
            }
        }

        private void InitializePlugin()
        {
            Name = this.GetType().Name;
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            AddPage(new MainPage(this, Name, Version));
            AddPage(new JoinChannelPage(this));
            AddPage(new LeaveChannelPage(this));
            AddPage(new StopListeningChannelPage(this));
            SetPage<MainPage>();
            
            OnPluginLoad();
        }

        private const string DebugChannel = "ADH-DEBUG";

        private BasePlugin(string firstChannel, IEnumerable<string> allChannels, bool debug = false) : base($"Console host", breadcrumbHeader: true)
        {
            IsInDebug = debug;
            if (!IsInDebug && firstChannel == DebugChannel)
            {
                throw new Exception("If you don't want to use the debug mode, use another constructor.");
            }

            InitializePlugin();
            if (!IsInDebug)
            {
                var missingJoinedChannels = Channels.Select(x => x.ToLower()).Except(FChatClient.State.Channels.Select(x => x.ToLower()));
                foreach (var missingChannel in missingJoinedChannels)
                {
                    FChatClient.JoinChannel(missingChannel);
                }
            }
            //get the type of the class
            FieldInfo finfo = this.GetType().BaseType.BaseType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).First();
            finfo.SetValue(this, $"{Name} ({Version})");
            Run();

        }

        /// <summary>
        /// Should only be used for debug purposes.
        /// </summary>
        public BasePlugin(bool debug) : this(DebugChannel, new List<string>() { DebugChannel }, debug)
        {
        }

        public BasePlugin(string channel, bool debug = false) : this(channel, new List<string>() { channel }, debug)
        {
        }

        public BasePlugin(IEnumerable<string> channels, bool debug = false) : this(channels.First(), channels, debug)
        {
        }

        private void ReceivedCommand(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var unparsedMessage = Encoding.UTF8.GetString(body);
            try
            {
                var deserializedObject = JsonConvert.DeserializeObject<ReceivedPluginCommandEventArgs>(unparsedMessage);
                
            }
            catch (Exception ex)
            {
                return;
            }

        }

        private bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }

        public virtual List<string> GetCommandList()
        {
            var type = typeof(BaseCommand<>);
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => IsAssignableToGenericType(p, typeof(BaseCommand<>)));
            var listOfTypes = types.Select(x => x.Name).Where(x => x != "BaseCommand`1").Distinct();
            return listOfTypes.ToList();
        }

        public bool DoesCommandExist(string command)
        {
            return GetCommandList().Exists(x => x.ToLower() == command.ToLower());
        }

        public bool ExecuteCommand(string characterCalling, string command, string[] args, string channel)
        {
            if (DoesCommandExist(command))
            {
                try
                {
                    var thisType = this.GetType();
                    var searchedType = typeof(BaseCommand<>);
                    var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => IsAssignableToGenericType(p, typeof(BaseCommand<>)));
                    var typeToCreate = types.FirstOrDefault(x => x.Name.ToLower() == command.ToLower());
                    if (typeToCreate != null)
                    {
                        var instance = Activator.CreateInstance(typeToCreate);
                        instance.GetType().InvokeMember(nameof(BaseCommand<DummyPlugin>.Plugin), BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty, Type.DefaultBinder, instance, new object[] { this });
                        instance.GetType().InvokeMember(nameof(BaseCommand<DummyPlugin>.ExecuteCommand), BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, Type.DefaultBinder, instance, new object[] { characterCalling, args, channel });
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }

                return true;
            }
            return false;
        }

        public void OnPluginLoad()
        {
            PluginId = System.Guid.NewGuid();
            FChatClient = new RemoteBotController();

            if (!IsInDebug)
            {
                FChatClient.Connect();

                FChatClient.Events.ReceivedChatCommand += Events_ReceivedChatCommand;

                while (FChatClient.State == null || !FChatClient.State.IsBotReady)
                {
                    Console.WriteLine("Awaiting connection to the Host... Make sure you've started at least one instance of FChatSharpHost (or Bot, if you know what you're doing).");
                    Task.Delay(1000).ConfigureAwait(false);
                }

                Console.WriteLine("Connected!");
            }
            else
            {
                Console.WriteLine($"Debug mode activated. 'Joining' the default debug channel {DebugChannel}");
            }

        }

        private void Events_ReceivedChatCommand(object sender, ReceivedPluginCommandEventArgs e)
        {
            if (Channels.Contains(e.Channel, StringComparer.OrdinalIgnoreCase))
            {
                ExecuteCommand(e.Character, e.Command, e.Arguments, e.Channel);
            }
        }

        public void OnPluginUnload()
        {
            FChatClient.Events.ReceivedChatCommand -= Events_ReceivedChatCommand;
        }

        public void AddHandledChannel(string channel)
        {
            Channels.Add(channel);
        }

        public void RemoveHandledChannel(string channel)
        {
            Channels.RemoveAll(x => x.ToLower() == channel.ToLower());
        }
    }
}
