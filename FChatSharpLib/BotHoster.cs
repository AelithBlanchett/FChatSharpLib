using EasyConsole;
using FChatSharpLib.GUI;
using FChatSharpLib.GUI.Host;
using System;
using System.Collections.Generic;
using System.Text;

namespace FChatSharpLib
{
    public class BotHoster : Program
    {
        public Bot Bot;

        public BotHoster(string username, string password, string botCharacterName, string administratorCharacterName, bool debug, int delayBetweenEachReconnection) : base("FChatSharpLib - Host", breadcrumbHeader: true)
        {
            AddPage(new MainPage(this));
            AddPage(new JoinChannelPage(this));
            AddPage(new LeaveChannelPage(this));
            AddPage(new EnableDisableDebugMode(this));
            SetPage<MainPage>();
            Bot = new Bot(username, password, botCharacterName, administratorCharacterName, debug, delayBetweenEachReconnection);
            Bot.Connect();
            Run();
        }
    }
}
