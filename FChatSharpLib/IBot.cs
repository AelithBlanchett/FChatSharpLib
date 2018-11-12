﻿using FChatSharpLib.Entities.Events.Helpers;
using System.Collections.Generic;
using WebSocketSharp;

namespace FChatSharpLib
{
    public interface IBot
    {
        Events Events { get; set; }
        State State { get; set; }

        void Connect();
        void CreateChannel(string channelTitle);
        void Disconnect();
        bool IsUserAdmin(string character, string channel);
        bool IsUserMaster(string character);
        bool IsUserOP(string character, string channel);
        bool IsSelf(string character);
        void JoinChannel(string channel);
        void KickUser(string character, string channel);
        void SendMessage(string message, string channel);
    }
}