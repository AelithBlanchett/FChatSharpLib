using FChatSharpLib.Entities.Events.Helpers;
using System;
using System.Collections.Generic;
using WebSocketSharp;

namespace FChatSharpLib
{
    public interface IBot
    {
        IEvents Events { get; set; }
        State State { get; set; }

        void Connect();
        void Disconnect();

        bool IsUserAdmin(string character, string channel);
        bool IsUserMaster(string character);
        bool IsUserOP(string character, string channel);
        bool IsSelf(string character);

        void JoinChannel(string channel);
        void CreateChannel(string channelTitle);
        void SendMessageInChannel(string message, string channel);
        void SendPrivateMessage(string message, string character);
        void KickUser(string character, string channel);
        void InviteUserToChannel(string character, string channel);
        void BanUser(string character, string channel);
        void ChangeChannelDescription(string channel, string description);
        void ChangeChannelPrivacy(string channel, bool isPrivate);

        void SendCommandToServer(string data);

        event EventHandler<Entities.Events.Server.JoinChannel> UserJoinedChannel;
        event EventHandler<Entities.Events.Server.InitialChannelData> BotJoinedChannel;
        event EventHandler<Entities.Events.Server.StatusChanged> UserChangedStatus;
        event EventHandler<Entities.Events.Server.OnlineNotification> UserLoggedOn;
        event EventHandler<Entities.Events.Server.OfflineNotification> UserLoggedOff;
        event EventHandler<Entities.Events.Server.LeaveChannel> UserLeftChannel;
        event EventHandler<Entities.Events.Server.AddedChanOP> AddedOPInChannel;
        event EventHandler<Entities.Events.Server.RemovedChanOP> RemovedOPInChannel;
        event EventHandler<Entities.Events.Server.InitialChannelData> BotCreatedChannel;
    }
}