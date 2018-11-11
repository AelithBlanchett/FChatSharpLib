using FChatSharpLib.Entities.EventHandlers;
using FChatSharpLib.Entities.Plugin.Commands;
using FChatSharpLib.Plugin;
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
        public abstract string Name { get; }
        public abstract string Version { get; }
        public Guid PluginId { get; set; }

        public BasePlugin()
        {
            OnPluginLoad();
        }

        private void ReceivedCommand(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var unparsedMessage = Encoding.UTF8.GetString(body);
            try
            {
                var deserializedObject = JsonConvert.DeserializeObject<ReceivedPluginCommandEventArgs>(unparsedMessage);
                Console.WriteLine($"received: {deserializedObject.Command} in {deserializedObject.Channel} from {deserializedObject.Character} with args: {deserializedObject.Arguments}");
                Console.WriteLine(" BasePlugin Received {0}", deserializedObject);
                if (FChatClient.Channels.Any(x => x.ToLower() == deserializedObject.Channel.ToLower()))
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
            var listOfTypes = types.Select(x => x.Name).Where(x => x != "ICommand" && x != "BaseCommand").Distinct();
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
        }

        public void OnPluginUnload()
        {
            _pubsubChannel.Close();
        }
    }
}
