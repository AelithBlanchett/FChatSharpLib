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
    [Serializable]
    public abstract class BasePlugin : MarshalByRefObject, IPlugin
    {
        public IBot FChatClient { get; set; }
        public abstract string Name { get; }
        public abstract string Version { get; }
        public string Channel { get; set; }

        private Guid _pluginId;

        public Guid PluginId
        {
            get
            {
                return _pluginId;
            }

            set
            {
                _pluginId = value;
            }
        }

        

        private IModel _pubsubChannel;


        private void ReceivedCommand(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var unparsedMessage = Encoding.UTF8.GetString(body);
            try
            {
                var deserializedObject = JsonConvert.DeserializeObject<ReceivedPluginCommandEventArgs>(unparsedMessage);
                Console.WriteLine($"received: {deserializedObject.Command} in {deserializedObject.Channel} from {deserializedObject.Character} with args: {deserializedObject.Arguments}");
                Console.WriteLine(" BasePlugin Received {0}", deserializedObject);
                if (deserializedObject.Channel.ToLower() == Channel.ToLower())
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
                        ICommand instance = (ICommand)Activator.CreateInstance(typeToCreate, this);
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

        //Use this instead of the constructor
        public void OnPluginLoad(string channel)
        {
            PluginId = System.Guid.NewGuid();
            FChatClient = new RemoteBotController();
            Channel = channel;

            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            _pubsubChannel = connection.CreateModel();
            _pubsubChannel.QueueDeclare(queue: "FChatLib.Plugins.ToPlugins",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
            var consumer = new EventingBasicConsumer(_pubsubChannel);
            consumer.Received += ReceivedCommand;
            _pubsubChannel.BasicConsume(queue: "FChatLib.Plugins.ToPlugins",
                                 autoAck: true,
                                 consumer: consumer);
        }

        public void OnPluginUnload()
        {
            _pubsubChannel.Close();
        }
    }
}
