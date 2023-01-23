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
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using FChatSharpLib.Entities.Events;
using System.Threading;
using System.Threading.Tasks;
using FChatSharpLib.Entities.Events.Helpers;
using System.Linq;
using FChatSharpLib.Entities.EventHandlers.FChatEvents;
using System.Globalization;
using Microsoft.Extensions.Options;

namespace FChatSharpLib
{
    public class Bot : BaseBot
    {
        private Timer _stateUpdateMonitor;

        public IOptions<FChatSharpHostOptions> HostOptions { get; }

        public Bot(IOptions<FChatSharpHostOptions> hostOptions, Events events) : base(events)
        {
            HostOptions = hostOptions;
            if(hostOptions.Value == null)
            {
                throw new InvalidOperationException("The configuration options are missing.");
            }
            State.AdminCharacterName = HostOptions.Value.AdministratorCharacterName;
            State.BotCharacterName = HostOptions.Value.BotCharacterName;
        }

        public override void Connect()
        {
            Events.ReceivedPluginRawData += Events_ReceivedPluginRawData;
            base.Connect();
            _stateUpdateMonitor = new Timer(AutoUpdateState, null, 0, 2000);
        }

        private void AutoUpdateState(object state)
        {
            if (State.IsBotReady)
            {
                DefaultFChatEventHandler.ReceivedStateUpdate?.Invoke(this, new Entities.EventHandlers.ReceivedStateUpdateEventArgs()
                {
                    State = State,
                });
            }
        }

        private void Events_ReceivedPluginRawData(object sender, Entities.EventHandlers.ReceivedPluginRawDataEventArgs e)
        {
            SendCommandToServer(e.jsonData);
        }

        public override void Events_ReceivedTriggeringEvent(object sender, Entities.EventHandlers.ReceivedEventEventArgs e)
        {
            var triggered = true;
            switch (e.Event.GetType().Name)
            {
                case nameof(FChatSharpLib.Entities.Events.Server.JoinChannel):
                    var jchEvent = (FChatSharpLib.Entities.Events.Server.JoinChannel)e.Event;
                    State.AddCharacterInChannel(jchEvent.channel, jchEvent.character.identity);
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.InitialChannelData):
                    var ichEvent = (FChatSharpLib.Entities.Events.Server.InitialChannelData)e.Event;
                    foreach (var character in ichEvent.users)
                    {
                        State.AddCharacterInChannel(ichEvent.channel, character.identity);
                    }
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.ConnectedUsers):
                    var conEvent = (FChatSharpLib.Entities.Events.Server.ConnectedUsers)e.Event;
                    State.IsBotReady = true;
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.ListConnectedUsers):
                    var listEvent = (FChatSharpLib.Entities.Events.Server.ListConnectedUsers)e.Event;
                    foreach (var characterState in listEvent.characters)
                    {
                        var stateExists = State.CharactersInfos.TryGetValue(characterState.Character.ToLower(), out var existingState);
                        if (stateExists == false)
                        {
                            State.CharactersInfos.TryAdd(characterState.Character.ToLower(), characterState);
                        }
                        else
                        {
                            existingState.Gender = characterState.Gender;
                            existingState.Status = characterState.Status;
                            existingState.StatusText = characterState.StatusText;
                            existingState.LastUpdate = DateTime.UtcNow;
                        }
                    }
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.StatusChanged):
                    var staEvent = (FChatSharpLib.Entities.Events.Server.StatusChanged)e.Event;
                    if(State.CharactersInfos.TryGetValue(staEvent.character.ToLower(), out var charInfoSta))
                    {
                        charInfoSta.Status = FChatEventParser.GetEnumEquivalent<StatusEnum>(staEvent.status.ToLower());
                        charInfoSta.StatusText = charInfoSta.StatusText;
                        charInfoSta.LastUpdate = DateTime.UtcNow;
                    }
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.OnlineNotification):
                    State.IsBotReady = true;
                    var nlnEvent = (FChatSharpLib.Entities.Events.Server.OnlineNotification)e.Event;

                    var charInfoNlnExists = State.CharactersInfos.TryGetValue(nlnEvent.identity.ToLower(), out var charInfoNln);
                    if (charInfoNlnExists == false)
                    {
                        charInfoNln = new CharacterState()
                        {
                            Character = nlnEvent.identity,
                            Gender = FChatEventParser.GetEnumEquivalent<GenderEnum>(nlnEvent.gender.ToLower()),
                            Status = FChatEventParser.GetEnumEquivalent<StatusEnum>(nlnEvent.status.ToLower()),
                            LastUpdate = DateTime.UtcNow
                        };
                        State.CharactersInfos.TryAdd(charInfoNln.Character.ToLower(), charInfoNln);
                    }
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.OfflineNotification):
                    var flnEvent = (FChatSharpLib.Entities.Events.Server.OfflineNotification)e.Event;
                    var charInfoFln = State.CharactersInfos.TryRemove(flnEvent.character.ToLower(), out var removedCharacter);
                    State.ChannelsInfo.Values.ToList().ForEach(x => x.CharactersInfo.RemoveAll(y => y.Character.ToLower() == flnEvent.character.ToLower()));
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.LeaveChannel):
                    var lchEvent = (FChatSharpLib.Entities.Events.Server.LeaveChannel)e.Event;
                    if(!State.ChannelsInfo.TryGetValue(lchEvent.channel.ToLower(), out var channelState))
                    {
                        return;
                    }
                    
                    channelState.CharactersInfo.RemoveAll(y => y.Character.ToLower() == lchEvent.character.ToLower());
                    if (this.IsSelf(lchEvent.character))
                    {
                        State.ChannelsInfo.TryRemove(lchEvent.channel.ToLower(), out var x);
                    }
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.ChannelOperators):
                    var colEvent = (FChatSharpLib.Entities.Events.Server.ChannelOperators)e.Event;
                    if (!State.ChannelsInfo.TryGetValue(colEvent.channel.ToLower(), out var chanOPChannelState))
                    {
                        return;
                    }
                    chanOPChannelState.Operators = colEvent.oplist;
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.AddedChanOP):
                    var coaEvent = (FChatSharpLib.Entities.Events.Server.AddedChanOP)e.Event;
                    if (!State.ChannelsInfo.TryGetValue(coaEvent.channel.ToLower(), out var addedChanOPChannelState))
                    {
                        return;
                    }
                    addedChanOPChannelState.Operators.Add(coaEvent.character);
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.RemovedChanOP):
                    var corEvent = (FChatSharpLib.Entities.Events.Server.RemovedChanOP)e.Event;
                    if (!State.ChannelsInfo.TryGetValue(corEvent.channel.ToLower(), out var removedChanOPchannelState))
                    {
                        return;
                    }
                    removedChanOPchannelState.Operators.RemoveAll(x => x == corEvent.character.ToLower());
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.Ping):
                    SendPing(null);
                    break;
                case nameof(FChatSharpLib.Entities.Events.Server.VariableReceived):
                    var varEvent = (FChatSharpLib.Entities.Events.Server.VariableReceived)e.Event;
                    switch (varEvent.variable)
                    {
                        case "msg_flood":
                            Events.SetFloodLimit(double.Parse(varEvent.value, CultureInfo.InvariantCulture));
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    triggered = false;
                    break;
            }


            //Disabled for performance
            //if (triggered)
            //{
            //    DefaultFChatEventHandler.ReceivedStateUpdate?.Invoke(this, new Entities.EventHandlers.ReceivedStateUpdateEventArgs()
            //    {
            //        State = State
            //    });
            //}

            base.Events_ReceivedTriggeringEvent(sender, e);
        }

        //Misc

        public void SendPing(Object stateInfo)
        {
            SendCommandToServer(new Ping()
            {
            }.ToString());
        }

    }
}
