# FChatSharp

A simple F-Chat (F-List.net's chat service) C# bot & library with a plugin system, communicating by RabbitMQ.

## Quick-start
  - Install the [.NET 6.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
  - Install [RabbitMQ > 3.0](https://www.rabbitmq.com/download.html). Docker might be the best option.
  - Download and unzip the latest [FChatSharp.Host release](https://github.com/AelithBlanchett/FChatSharp.Host/releases/)
  - Edit the appsettings.json, enter your credentials, the character names, and the RabbitMQ instance infos.
  - Make sure to start your RabbitMQ instance first. Note: you'll have to create a new user with admin permissions if it's not running on 127.0.0.1.
  - Run the FChatSharp.Host.exe
  - Run any other plugins once the host is started. If the host goes down, the plugins will automatically reconnect to it.

## How does it work?

```mermaid
graph TD;
    fchat(F-Chat Event)-->Host
    Host --> RabbitMQ;
    RabbitMQ --> PluginA;
    RabbitMQ <--> PluginB;
    PluginA --> RabbitMQ;
    PluginB --> RabbitMQ;
```
1. A [FChatSharp.Host](https://github.com/AelithBlanchett/FChatSharp.Host) instance connects to F-chat using your credentials and specified character name.
2. The host receives all the events sent by the F-chat server (new messages, who went online, who joined your channel).
3. The host reads those events, and forwards them to the existing plugins listening to the RabbitMQ message queue.
4. A plugin ([FChatSharp.ExamplePlugin](https://github.com/AelithBlanchett/FChatSharp.ExamplePlugin)) connected to the same RabbitMQ message queue will make actions depending on the event received (most probably a command like !command), and can interact with F-chat through the FChatClient provided inside the class of that plugin.
5. For all the actions relating to F-chat, they are then forwarded back to the Host.
6. The host is the one making the action.

### Support or Contact

Contact Elise Pariat / Aelith Blanchette on F-list/F-chat.
