How to add more events:

State modifying events (characters, statuses, ops, kicks): Bot.Events_ReceivedStateModifyingEvent
For plugin-side events: RemoteBotController.RelayServerEvents
To add new bot command: IBot.MyNewFeatureUsingAClientEvent && PluginManager.ForwardReceivedCommandToBot