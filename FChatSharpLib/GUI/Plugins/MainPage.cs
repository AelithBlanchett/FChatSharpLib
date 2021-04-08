using EasyConsole;
using FChatSharpLib.Entities.Plugin;
using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.GUI.Plugins
{
    class MainPage : MenuPage
    {
        public MainPage(BasePlugin program, string pluginName, string pluginVersion)
            : base($"{pluginName} ({pluginVersion}) - Main Menu", program,
                  new Option("Join a channel", () => program.NavigateTo<JoinChannelPage>()),
                  new Option("Leave a channel", () => program.NavigateTo<LeaveChannelPage>()),
                  new Option("Execute a command", () => program.NavigateTo<ExecuteCommandPage>()),
                  new Option("Stop listening to commands in a channel", () => program.NavigateTo<StopListeningChannelPage>()),
                  new Option("Broadcast a message", () => program.NavigateTo<BroadcastMessagePage>()),
                  new Option("Send a message", () => program.NavigateTo<SendMessagePage>()))
        {
        }
    }
}
