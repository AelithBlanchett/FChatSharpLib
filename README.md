# FChatSharpLib

Requirements:

-Requires RabbitMQ Server to be installed and running on the host machine.

-RabbitMQ requires a 64-bit supported version of Erlang for Windows to be installed.

-NetcoreApp 2.1 - does not work on versions of Visual Studio earlier than 2017

-See F-Chat protocol for rules regarding using a Bot in F-Chat here https://wiki.f-list.net/F-Chat_Protocol#Bots

Functions:

-Connect to the F-Chat server

-Join and leave channels

-Send chat messages to users or channels

-Update bot status

-Ban and unban users

-Listen for messages and user status updates


Connect with:
var bot = new FChatSharpHost("FChat_account_username", "FChat_account_password", "FChat_character_name", "administrator_character_name", true, 4000);

Join public channels with: 
Bot.JoinChannel("Development"); //Official channels are joined with the channel name
Bot.JoinChannel("adh-7df6e9bffad6ca4e07e3"); //Custom channels are joined with the room code

(remember that to join a channel with the code you must have an invite or the channel must be public)

Send chat messages to characters with:
Bot.SendPrivateMessage("hello!", "Character_name");

Send chat messages to channels with:
Bot.SendMessageInChannel("hello channel!", "adh-7df6e9bffad6ca4e07e3");


See ExamplePlugin project for an implementation of a plugin bot that uses FChatSharpLib.
