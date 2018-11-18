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
using System.Text;
using System.Threading.Tasks;

namespace FChatSharpLib.Entities.Plugin
{
    public abstract class BasePlugin : Program, IPlugin
    {
        public BaseBot FChatClient { get; set; }
        public string Channel { get; set; }
        public List<string> Channels { get; set; }
        public abstract string Name { get; }
        public abstract string Version { get; }
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

        public BasePlugin(string channel) : base("FChatSharpLib - Plugin", breadcrumbHeader: true)
        {
            AddPage(new MainPage(this));
            AddPage(new JoinChannelPage(this));
            AddPage(new LeaveChannelPage(this));
            AddPage(new StopListeningChannelPage(this));
            SetPage<MainPage>();
            OnPluginLoad();
            Channel = channel;
            Channels = new List<string>() { channel };
            if (!FChatClient.State.Channels.Any(x => x.ToLower() == channel.ToLower()))
            {
                FChatClient.JoinChannel(channel);
            }
            Run();
        }

        public BasePlugin(IEnumerable<string> channels) : base("FChatSharpLib - Plugin", breadcrumbHeader: true)
        {
            OnPluginLoad();
            Channels = channels.ToList();
            Channel = channels.First();
            var missingJoinedChannels = channels.Select(x => x.ToLower()).Except(FChatClient.State.Channels.Select(x => x.ToLower()));
            foreach (var missingChannel in missingJoinedChannels)
            {
                FChatClient.JoinChannel(missingChannel);
            }
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

        public virtual List<string> GetCommandList()
        {
            var type = typeof(ICommand);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ICommand).IsAssignableFrom(p));
            var listOfTypes = types.Select(x => x.Name).Where(x => x != nameof(ICommand) && x != nameof(BaseCommand)).Distinct();
            return listOfTypes.ToList();
        }

        public bool DoesCommandExist(string command)
        {
            return GetCommandList().Exists(x => x.ToLower() == command.ToLower());
        }

        public bool ExecuteCommand(string command, string[] args, string channel)
        {
            if (DoesCommandExist(command))
            {
                try
                {
                    var searchedType = typeof(ICommand);
                    var types = AppDomain.CurrentDomain.GetAssemblies()
                                .SelectMany(s => s.GetTypes())
                                .Where(p => typeof(ICommand).IsAssignableFrom(p));
                    var typeToCreate = types.FirstOrDefault(x => x.Name.ToLower() == command.ToLower());
                    if (typeToCreate != null)
                    {
                        ICommand instance = (ICommand)Activator.CreateInstance(typeToCreate);
                        instance.MyPlugin = this;
                        instance.ExecuteCommand(args, channel);
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

            FChatClient.Connect();

            FChatClient.Events.ReceivedChatCommand += Events_ReceivedChatCommand;

            while (FChatClient.State == null || !FChatClient.State.IsBotReady)
            {
                Task.Delay(1000).ConfigureAwait(false);
            }
        }

        private void Events_ReceivedChatCommand(object sender, ReceivedPluginCommandEventArgs e)
        {
            if (Channels.Contains(e.Channel, StringComparer.OrdinalIgnoreCase))
            {
                ExecuteCommand(e.Command, e.Arguments, e.Channel);
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
