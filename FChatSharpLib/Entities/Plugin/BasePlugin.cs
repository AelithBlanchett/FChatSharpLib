using EasyConsole;
using FChatSharpLib.Entities.EventHandlers;
using FChatSharpLib.Entities.Plugin.Commands;
using FChatSharpLib.GUI.Plugins;
using Microsoft.Extensions.Options;
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
        public string Version { get; set; }
        public Guid PluginId { get; set; }
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

        public IOptions<FChatSharpPluginOptions> Options { get; }

        private void InitializePlugin()
        {
            Name = this.GetType().Name;
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            AddPage(new MainPage(this, Name, Version));
            AddPage(new JoinChannelPage(this));
            AddPage(new LeaveChannelPage(this));
            AddPage(new ExecuteCommandPage(this));
            AddPage(new StopListeningChannelPage(this));
            AddPage(new BroadcastMessagePage(this));
            AddPage(new SendMessagePage(this));
            SetPage<MainPage>();
        }

        private const string DebugChannel = "ADH-DEBUG";

        public BasePlugin(IOptions<FChatSharpPluginOptions> options, RemoteBotController fChatClient) : base($"Console host", breadcrumbHeader: true)
        {
            Options = options;
            FChatClient = fChatClient;
            Channel = Options.Value.Channels.First();
            Channels = Options.Value.Channels;

            InitializePlugin();
            OnPluginLoad();
            Console.WriteLine("Loaded commands: " + string.Join(", ", GetCommandList()));

            //get the type of the class
            var finfos = GetFieldInfosIncludingBaseClasses(this.GetType(), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            FieldInfo finfo = finfos.First(x => x.DeclaringType.FullName == "EasyConsole.Program" && x.Name == "<Title>k__BackingField");
            finfo.SetValue(this, $"{Name} ({Version})");
            
        }

        private void ReceivedCommand(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var unparsedMessage = Encoding.UTF8.GetString(body.ToArray());
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
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => p.IsClass && !p.IsAbstract && p.IsPublic && IsAssignableToGenericType(p, typeof(BaseCommand<>)));
            var listOfTypes = types.Select(x => x.Name.Split("`").FirstOrDefault()).Where(x => x != "BaseCommand`1").Distinct();
            return listOfTypes.ToList();
        }

        public bool DoesCommandExist(string command)
        {
            return GetCommandList().Exists(x => x.ToLower() == command.ToLower());
        }

        public bool ExecuteCommand(string characterCalling, string command, IEnumerable<string> args, string channel)
        {
            command = command.ToLower();

            if (DoesCommandExist(command))
            {
                try
                {
                    var thisType = this.GetType();
                    var searchedType = typeof(BaseCommand<>);
                    var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => IsAssignableToGenericType(p, typeof(BaseCommand<>)));
                    var typeToCreate = types.FirstOrDefault(x => x.Name.Split("`").FirstOrDefault().ToLower() == command.ToLower());
                    if (typeToCreate != null)
                    {
                        if (this.GetType().BaseType != null && this.GetType().BaseType.IsGenericType && typeToCreate.IsGenericType)
                        {
                            var genericArgs = this.GetType().BaseType.GenericTypeArguments;
                            typeToCreate = typeToCreate.MakeGenericType(genericArgs);
                        }

                        var instance = Activator.CreateInstance(typeToCreate);
                        instance.GetType().InvokeMember(nameof(BaseCommand<DummyPlugin>.Plugin), BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty, Type.DefaultBinder, instance, new object[] { this });
                        Task result = null;
                        if (!string.IsNullOrWhiteSpace(channel))
                        {
                            result = (Task)instance.GetType().InvokeMember(nameof(BaseCommand<DummyPlugin>.ExecuteCommand), BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, Type.DefaultBinder, instance, new object[] { characterCalling, args, channel });
                        }
                        else
                        {
                            result = (Task)instance.GetType().InvokeMember(nameof(BaseCommand<DummyPlugin>.ExecutePrivateCommand), BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, Type.DefaultBinder, instance, new object[] { characterCalling, args });
                        }

                        if (result.Status == TaskStatus.Faulted)
                        {
                            if (result.Exception.InnerException != null)
                            {
                                FChatClient.SendMessageInChannel($"Error: {result.Exception.InnerException.Message}", channel);
                            }
                            else
                            {
                                FChatClient.SendMessageInChannel($"Error: {result.Exception.Message}", channel);
                            }
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (FChatClient != null && FChatClient.State != null && FChatClient.State.IsBotReady)
                    {
                        if (ex.InnerException != null)
                        {
                            FChatClient.SendMessageInChannel($"Internal error: {ex.InnerException.Message}", channel);
                        }
                        else
                        {
                            FChatClient.SendMessageInChannel($"Internal error: {ex.Message}", channel);
                        }
                    }
                    return false;
                }

                return true;
            }
            return false;
        }

        public void OnPluginLoad()
        {
            PluginId = System.Guid.NewGuid();

            if (!Options.Value.Debug)
            {
                FChatClient.Connect();

                FChatClient.Events.ReceivedChatCommand += Events_ReceivedChatCommand;
                FChatClient.BotConnected += FChatClient_BotConnected;
                FChatClient.Events.ReceivedStateUpdate += Events_ReceivedStateUpdate;

                Console.WriteLine("Awaiting connection to the Host... Make sure you've started at least one instance of FChatSharpHost (or Bot, if you know what you're doing).");
            }
            else
            {
                Console.WriteLine($"Debug mode activated. 'Joining' the default debug channel {DebugChannel}");
            }

        }

        public DateTime LastTimeJoinMissingChannelsCalled = DateTime.MinValue;

        private void Events_ReceivedStateUpdate(object sender, ReceivedStateUpdateEventArgs e)
        {
            if ((DateTime.Now - LastTimeJoinMissingChannelsCalled).TotalMilliseconds > 5000)
            {
                LastTimeJoinMissingChannelsCalled = DateTime.Now;
                JoinRequiredChannels();
            }
        }

        private void FChatClient_BotConnected(object sender, EventArgs e)
        {
            Console.WriteLine("Connected!");
            JoinRequiredChannels(false);
        }

        private void JoinRequiredChannels(bool excludeAlreadyJoinedOnes = true)
        {
            if (!Options.Value.Debug)
            {
                var missingJoinedChannels = Channels.Select(x => x.ToLower());
                if (excludeAlreadyJoinedOnes)
                {
                    missingJoinedChannels = missingJoinedChannels.Except(FChatClient.State.Channels.Select(x => x.ToLower()));
                }
                foreach (var missingChannel in missingJoinedChannels)
                {
                    Console.WriteLine($"Joining channel {missingChannel}");
                    FChatClient.JoinChannel(missingChannel);
                }
            }
        }

        private void Events_ReceivedChatCommand(object sender, ReceivedPluginCommandEventArgs e)
        {
            if (Channels.Contains(e.Channel, StringComparer.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(e.Channel)) //Null or whitespace=  private command
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

        /// <summary>
        ///   Returns all the fields of a type, working around the fact that reflection
        ///   does not return private fields in any other part of the hierarchy than
        ///   the exact class GetFields() is called on.
        /// </summary>
        /// <param name="type">Type whose fields will be returned</param>
        /// <param name="bindingFlags">Binding flags to use when querying the fields</param>
        /// <returns>All of the type's fields, including its base types</returns>
        public static FieldInfo[] GetFieldInfosIncludingBaseClasses(
            Type type, BindingFlags bindingFlags
        )
        {
            FieldInfo[] fieldInfos = type.GetFields(bindingFlags);

            // If this class doesn't have a base, don't waste any time
            if (type.BaseType == typeof(object))
            {
                return fieldInfos;
            }
            else
            { // Otherwise, collect all types up to the furthest base class
                var fieldInfoList = new List<FieldInfo>(fieldInfos);
                while (type.BaseType != typeof(object))
                {
                    type = type.BaseType;
                    fieldInfos = type.GetFields(bindingFlags);

                    // Look for fields we do not have listed yet and merge them into the main list
                    for (int index = 0; index < fieldInfos.Length; ++index)
                    {
                        bool found = false;

                        for (int searchIndex = 0; searchIndex < fieldInfoList.Count; ++searchIndex)
                        {
                            bool match =
                                (fieldInfoList[searchIndex].DeclaringType == fieldInfos[index].DeclaringType) &&
                                (fieldInfoList[searchIndex].Name == fieldInfos[index].Name);

                            if (match)
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            fieldInfoList.Add(fieldInfos[index]);
                        }
                    }
                }

                return fieldInfoList.ToArray();
            }
        }
    }
}
