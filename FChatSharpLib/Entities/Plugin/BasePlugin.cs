using FChatSharpLib.Entities.EventHandlers;
using FChatSharpLib.Entities.Plugin.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Plugin
{
    public abstract class BasePlugin : IPlugin
    {
        public BaseBot FChatClient { get; set; }
        public ILogger<BasePlugin> Logger { get; }
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
        }

        private const string DebugChannel = "ADH-DEBUG";

        public BasePlugin(IOptions<FChatSharpPluginOptions> options, RemoteBotController fChatClient, ILogger<BasePlugin> logger)
        {
            Options = options;
            FChatClient = fChatClient;
            Logger = logger;
            Channel = Options.Value.Channels.First();
            Channels = Options.Value.Channels;

            InitializePlugin();
            OnPluginLoad();
            Logger.LogInformation("Loaded commands: " + string.Join(", ", GetCommandList()));
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
            var listOfTypes = types.Select(x => x.Name.Split('`').FirstOrDefault()).Where(x => x != "BaseCommand`1").Distinct();
            return listOfTypes.ToList();
        }

        public bool DoesCommandExist(string command)
        {
            return GetCommandList().Exists(x => x.ToLower() == command.ToLower());
        }

        public async Task<bool> TryExecuteCommand(string characterCalling, string command, IEnumerable<string> args, string channel)
        {
            command = command.ToLower();
            Logger.LogInformation("Received potential command '{0}' sent by '{1}' in channel '{2}'.", command, characterCalling, channel);

            var executionSuccessful = true;

            if (!DoesCommandExist(command))
            {
                return false;
            }

            try
            {
                var thisType = this.GetType();
                var searchedType = typeof(BaseCommand<>);
                var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => IsAssignableToGenericType(p, typeof(BaseCommand<>)));
                var typeToCreate = types.FirstOrDefault(x => x.Name.Split('`').FirstOrDefault().ToLower() == command.ToLower());

                if (typeToCreate == null)
                {
                    Logger.LogError("There was an error executing the '{0}' command sent by '{1}' in channel '{2}': there is no class matching the name of that command.", command, characterCalling, channel);
                    return false;
                }

                if (this.GetType().BaseType != null && this.GetType().BaseType.IsGenericType && typeToCreate.IsGenericType)
                {
                    var genericArgs = this.GetType().BaseType.GenericTypeArguments;
                    typeToCreate = typeToCreate.MakeGenericType(genericArgs);
                }

                var instance = Activator.CreateInstance(typeToCreate);
                instance.GetType().InvokeMember(nameof(BaseCommand<DummyPlugin>.Plugin), BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty, Type.DefaultBinder, instance, new object[] { this });
                Task result = null;

                try
                {
                    if (!string.IsNullOrWhiteSpace(channel))
                    {
                        result = (Task)instance.GetType().InvokeMember(nameof(BaseCommand<DummyPlugin>.ExecuteCommand), BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, Type.DefaultBinder, instance, new object[] { characterCalling, args, channel });
                        await Task.WhenAny(result, Task.Delay(TimeSpan.FromSeconds(300)));
                    }
                    else
                    {
                        result = (Task)instance.GetType().InvokeMember(nameof(BaseCommand<DummyPlugin>.ExecutePrivateCommand), BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, Type.DefaultBinder, instance, new object[] { characterCalling, args });
                        await Task.WhenAny(result, Task.Delay(TimeSpan.FromSeconds(300)));
                    }

                    if (result.Status == TaskStatus.Faulted)
                    {
                        Logger.LogError(result.Exception, "There was an error executing the '{0}' command sent by '{1}' in channel '{2}'. The task failed to execute: {3}.", command, characterCalling, channel, result.Exception?.Message);
                        executionSuccessful = false;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "There was an error executing the '{0}' command sent by '{1}' in channel '{2}'.", command, characterCalling, channel);
                    executionSuccessful = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "There was a critical error building the '{0}' command sent by '{1}' in channel '{2}'.", command, characterCalling, channel);
                executionSuccessful = false;
            }

            if (!executionSuccessful)
            {
                FChatClient.SendPrivateMessage("There was an unexpected/unhandled error processing that command. Contact the administrator.", characterCalling);
            }

            return executionSuccessful;
        }

        public void OnPluginLoad()
        {
            PluginId = System.Guid.NewGuid();

            if (Options.Value.Debug)
            {
                Logger.LogInformation($"Debug mode activated. 'Joining' the default debug channel {DebugChannel}");
                return;
            }

            FChatClient.Events.ReceivedChatCommand += Events_ReceivedChatCommand;
            FChatClient.BotConnected += FChatClient_BotConnected;
            FChatClient.Events.ReceivedStateUpdate += Events_ReceivedStateUpdate;

            Logger.LogInformation("Awaiting connection to the Host... Make sure you've started at least one instance of FChatSharpHost (or Bot, if you know what you're doing).");
            FChatClient.Connect();
        }

        public DateTime LastTimeJoinMissingChannelsCalled = DateTime.MinValue;

        private void Events_ReceivedStateUpdate(object sender, ReceivedStateUpdateEventArgs e)
        {
            if ((DateTime.Now - LastTimeJoinMissingChannelsCalled).TotalMilliseconds > 30000)
            {
                LastTimeJoinMissingChannelsCalled = DateTime.Now;
                JoinRequiredChannels();
            }
        }

        private void FChatClient_BotConnected(object sender, EventArgs e)
        {
            Logger.LogInformation("Connected!");
            JoinRequiredChannels(false);
        }

        private void JoinRequiredChannels(bool excludeAlreadyJoinedOnes = true)
        {
            if (Options.Value.Debug)
            {
                return;
            }

            var originalChannels = Options.Value.Channels;
            var missingJoinedChannels = Channels.Select(x => x.ToLower());
            if (excludeAlreadyJoinedOnes)
            {
                missingJoinedChannels = missingJoinedChannels.Except(FChatClient.State.Channels.Select(x => x.ToLower())).ToList();
            }
            foreach (var missingChannel in missingJoinedChannels)
            {
                Logger.LogInformation($"Joining channel {missingChannel}");
                FChatClient.JoinChannel(missingChannel);

                if (!originalChannels.Contains(missingChannel))
                {
                    if (!LastJoinedChannels.ContainsKey(missingChannel))
                    {
                        LastJoinedChannels.Add(missingChannel, 1);
                    }
                    else
                    {
                        LastJoinedChannels[missingChannel]++;
                    }

                    if (LastJoinedChannels.TryGetValue(missingChannel, out var channelRetryCount))
                    {
                        if (channelRetryCount >= 3)
                        {
                            RemoveHandledChannel(missingChannel);
                            FChatClient.LeaveChannel(missingChannel);
                            LastJoinedChannels.Remove(missingChannel);
                        }
                    }
                }
            }
        }

        public Dictionary<string, int> LastJoinedChannels = new Dictionary<string, int>();

        private async void Events_ReceivedChatCommand(object sender, ReceivedPluginCommandEventArgs e)
        {
            if (Channels.Contains(e.Channel, StringComparer.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(e.Channel)) //Null or whitespace=  private command
            {
                await TryExecuteCommand(e.Character, e.Command, e.Arguments, e.Channel.ToLowerInvariant());
            }
        }

        public void OnPluginUnload()
        {
            FChatClient.Events.ReceivedChatCommand -= Events_ReceivedChatCommand;
        }

        public void AddHandledChannel(string channel)
        {
            Channels.Add(channel);
            Logger.LogInformation("Added channel {0} to the list of handled channels.", channel);
        }

        public void RemoveHandledChannel(string channel)
        {
            Channels.RemoveAll(x => x.ToLower() == channel.ToLower());
            Logger.LogInformation("Removed channel {0} from the list of handled channels.", channel);
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
