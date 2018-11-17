using FChatSharpLib.Entities.EventHandlers;
using FChatSharpLib.Entities.Plugin.Commands;
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
    public abstract class BasePlugin : IPlugin
    {
        private IModel _pubsubChannel;

        public IBot FChatClient { get; set; }
        public string Channel { get; set; }
        public IEnumerable<string> Channels { get; set; }
        public abstract string Name { get; }
        public abstract string Version { get; }
        public Guid PluginId { get; set; }
        public bool SingleChannelPlugin { get; set; }
        public int ChannelsConnectedTo
        {
            get
            {
                return Channels.Count();
            }
        }

        public BasePlugin(string channel)
        {
            OnPluginLoad();
            Channel = channel;
            Channels = new List<string>() { channel };
            SingleChannelPlugin = true;
            if (!FChatClient.State.Channels.Any(x => x.ToLower() == channel.ToLower()))
            {
                FChatClient.JoinChannel(channel);
            }
        }

        public BasePlugin(IEnumerable<string> channels)
        {
            OnPluginLoad();
            Channels = channels;
            Channel = channels.First();
            SingleChannelPlugin = false;
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
                if (Channel.ToLower() == deserializedObject.Channel.ToLower())
                {
                    ExecuteCommand(deserializedObject.Command, deserializedObject.Arguments);
                }
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
            var commandList = GetCommandList();
            return (commandList.FirstOrDefault(x => x.ToLower() == command.ToLower()) != null);
        }

        public bool ExecuteCommand(string command, string[] args)
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
                        instance.ExecuteCommand();
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

            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            _pubsubChannel = connection.CreateModel();
            _pubsubChannel.QueueDeclare(queue: "FChatSharpLib.Plugins.ToPlugins",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
            var consumer = new EventingBasicConsumer(_pubsubChannel);
            consumer.Received += ReceivedCommand;
            _pubsubChannel.BasicConsume(queue: "FChatSharpLib.Plugins.ToPlugins",
                                 autoAck: true,
                                 consumer: consumer);

            while (FChatClient.State == null || !FChatClient.State.IsBotReady)
            {
                Task.Delay(1000).ConfigureAwait(false);
            }
        }

        public void OnPluginUnload()
        {
            _pubsubChannel.Close();
        }
    }
}
