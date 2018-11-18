using EasyConsole;
using FChatSharpLib.Entities.Plugin;
using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib.GUI.Host
{
    class MainPage : MenuPage
    {
        public MainPage(BotHoster program)
            : base("FChatSharpLib - Host - Main Menu", program,
                  new Option("Join a channel", () => program.NavigateTo<JoinChannelPage>()),
                  new Option("Leave a channel", () => program.NavigateTo<LeaveChannelPage>()),
                  new Option($"Enable/Disable debug mode", () => program.NavigateTo<EnableDisableDebugMode>()))
        {
        }
    }
}
