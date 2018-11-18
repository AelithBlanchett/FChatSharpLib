using EasyConsole;
using FChatSharpLib.Entities.Plugin;
using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.GUI.Plugins
{
    class MainPage : MenuPage
    {
        public MainPage(BasePlugin program)
            : base("FChatSharpLib - Plugins - Main Menu", program,
                  new Option("Join a channel", () => program.NavigateTo<JoinChannelPage>()),
                  new Option("Leave a channel", () => program.NavigateTo<LeaveChannelPage>()),
                  new Option("Stop listening to commands in a channel", () => program.NavigateTo<StopListeningChannelPage>()))
        {
        }
    }
}
