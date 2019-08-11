using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using FChatSharpLib.Entities;
using FChatSharpLib.Entities.Events.Client;
using System.Collections.Specialized;
using FChatSharpLib.Entities.EventHandlers.WebSocket;
using FChatSharpLib.Entities.Plugin;
using System.Reflection;
using System.Security.Policy;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using FChatSharpLib.Entities.Events;
using System.Threading;
using System.Threading.Tasks;
using FChatSharpLib.Entities.Events.Helpers;
using System.Linq;

namespace FChatSharpLib
{

    public abstract class BaseBot
    {
        private int numberOfChannelsToTreatAsNewlyCreatedChannel = 0;

        public IEvents Events { get; set; }
        public State State { get; set; }

        public virtual void Connect()
        {
            Events.ReceivedFChatEvent += Events_ReceivedTriggeringEvent;
            Events.StartListening();
        }

        public virtual void Disconnect()
        {
            Events.StopListening();
        }

        public BaseBot(IEvents eventsHandler)
        {
            State = new State();
            Events = eventsHandler;
        }

        public virtual void Events_ReceivedTriggeringEvent(object sender, Entities.EventHandlers.ReceivedEventEventArgs e)
        {
            switch (e.Event.GetType().Name)
            {
                case nameof(FChatSharpLib.Entities.Events.Server.JoinChannel):
                    var jchEvent = (FChatSharpLib.Entities.Events.Server.JoinChannel)e.Event;
                    UserJoinedChannel?.Invoke(this, jchEvent);
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.InitialChannelData):
                    var ichEvent = (FChatSharpLib.Entities.Events.Server.InitialChannelData)e.Event;
                    BotJoinedChannel?.Invoke(this, ichEvent);
                    if (numberOfChannelsToTreatAsNewlyCreatedChannel > 0)
                    {
                        numberOfChannelsToTreatAsNewlyCreatedChannel--;
                        BotCreatedChannel?.Invoke(this, ichEvent);
                    }
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.ConnectedUsers):
                    var conEvent = (FChatSharpLib.Entities.Events.Server.ConnectedUsers)e.Event;
                    BotConnected?.Invoke(this, new Entities.EventHandlers.ReceivedStateUpdateEventArgs() { State = State });
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.StatusChanged):
                    var staEvent = (FChatSharpLib.Entities.Events.Server.StatusChanged)e.Event;
                    UserChangedStatus?.Invoke(this, staEvent);
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.OnlineNotification):
                    var nlnEvent = (FChatSharpLib.Entities.Events.Server.OnlineNotification)e.Event;
                    UserLoggedOn?.Invoke(this, nlnEvent);
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.OfflineNotification):
                    var flnEvent = (FChatSharpLib.Entities.Events.Server.OfflineNotification)e.Event;
                    UserLoggedOff?.Invoke(this, flnEvent);
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.LeaveChannel):
                    var lchEvent = (FChatSharpLib.Entities.Events.Server.LeaveChannel)e.Event;
                    UserLeftChannel?.Invoke(this, lchEvent);
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.AddedChanOP):
                    var coaEvent = (FChatSharpLib.Entities.Events.Server.AddedChanOP)e.Event;
                    AddedOPInChannel?.Invoke(this, coaEvent);
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.RemovedChanOP):
                    var corEvent = (FChatSharpLib.Entities.Events.Server.RemovedChanOP)e.Event;
                    RemovedOPInChannel?.Invoke(this, corEvent);
                    break;
                case nameof(Entities.Events.Server.RollResult):
                    var rllEvent = (FChatSharpLib.Entities.Events.Server.RollResult)e.Event;
                    RollResultReceived?.Invoke(this, rllEvent);
                    break;
                default:
                    break;
            }
        }


        //Events
        public event EventHandler<Entities.Events.Server.JoinChannel> UserJoinedChannel;
        public event EventHandler<Entities.Events.Server.InitialChannelData> BotJoinedChannel;
        public event EventHandler<Entities.Events.Server.StatusChanged> UserChangedStatus;
        public event EventHandler<Entities.Events.Server.OnlineNotification> UserLoggedOn;
        public event EventHandler<Entities.Events.Server.OfflineNotification> UserLoggedOff;
        public event EventHandler<Entities.Events.Server.LeaveChannel> UserLeftChannel;
        public event EventHandler<Entities.Events.Server.AddedChanOP> AddedOPInChannel;
        public event EventHandler<Entities.Events.Server.RemovedChanOP> RemovedOPInChannel;
        public event EventHandler<Entities.Events.Server.InitialChannelData> BotCreatedChannel;
        public event EventHandler<Entities.EventHandlers.ReceivedStateUpdateEventArgs> BotConnected;
        public event EventHandler<Entities.Events.Server.RollResult> RollResultReceived;


        // Permissions / Administration

        public bool IsUserAdmin(string character, string channel)
        {
            return (this.IsUserOP(character, channel) || this.IsUserMaster(character));
        }

        public bool IsUserMaster(string character)
        {
            return character.ToLower() == State.AdminCharacterName.ToLower();
        }

        public bool IsSelf(string character)
        {
            return character.ToLower() == State.BotCharacterName.ToLower();
        }

        public bool IsUserOP(string character, string channel)
        {
            return (State.ChannelsInfo.GetValueOrDefault(channel.ToLower()) != null ? State.ChannelsInfo.GetValueOrDefault(channel.ToLower()).Operators.Any(x => x.ToLower() == character.ToLower()) : false);
        }


        // Bot Commands

        public void JoinChannel(string channel)
        {
            SendCommandToServer(new JoinChannel()
            {
                channel = channel
            }.ToString());
        }

        public void CreateChannel(string channelTitle)
        {
            numberOfChannelsToTreatAsNewlyCreatedChannel++;
            SendCommandToServer(new CreateChannel()
            {
                channel = channelTitle
            }.ToString());
        }

        public void SendMessageInChannel(string message, string channel)
        {
            SendCommandToServer(new Message()
            {
                message = message,
                channel = channel
            }.ToString());
        }

        public void SendPrivateMessage(string message, string character)
        {
            SendCommandToServer(new PrivateMessage()
            {
                message = message,
                recipient = character
            }.ToString());
        }

        public void KickUser(string character, string channel)
        {
            SendCommandToServer(new KickFromChannel()
            {
                character = character,
                channel = channel
            }.ToString());
        }

        public void InviteUserToChannel(string character, string channel)
        {
            SendCommandToServer(new KickFromChannel()
            {
                character = character,
                channel = channel
            }.ToString());
        }

        public void BanUser(string character, string channel)
        {
            SendCommandToServer(new BanFromChannel()
            {
                character = character,
                channel = channel
            }.ToString());
        }

        public void ChangeChannelDescription(string description, string channel)
        {
            SendCommandToServer(new ChangeChannelDescription()
            {
                description = description,
                channel = channel
            }.ToString());
        }

        public void ChangeChannelPrivacy(bool isPrivate, string channel)
        {
            SendCommandToServer(new ChangeChannelPrivacy()
            {
                status = (isPrivate ? "private" : "public"),
                channel = channel
            }.ToString());
        }

        public void LeaveChannel(string channel)
        {
            SendCommandToServer(new LeaveChannel()
            {
                channel = channel
            }.ToString());
        }

        public void RollDice(string dice, string channel)
        {
            SendCommandToServer(new RollDice()
            {
                channel = channel,
                dice = dice
            }.ToString());
        }

        public void SpinBottle(string channel)
        {
            SendCommandToServer(new RollDice()
            {
                channel = channel,
                dice = "bottle"
            }.ToString());
        }

        public void SetStatus(StatusEnum status, string statusText)
        {
            SendCommandToServer(new SetStatus()
            {
                status = status.ToString(),
                statusmsg = statusText
            }.ToString());
        }

        public void UnbanUser(string character, string channel)
        {
            SendCommandToServer(new UnbanFromChannel()
            {
                character = character,
                channel = channel
            }.ToString());
        }

        public void TempBanUser(string character, int lengthInMinutes, string channel)
        {
            SendCommandToServer(new TempBanFromChannel()
            {
                character = character,
                channel = channel,
                length = lengthInMinutes.ToString()
            }.ToString());
        }


        //Misc

        public void SendCommandToServer(string data)
        {
            Events.SendCommand(data);
        }
    }
}
