using WebSocketSharp;

namespace FChatSharpLib
{
    public interface IBot
    {
        void Connect();
        void CreateChannel(string channelTitle);
        void Disconnect();
        bool IsUserAdmin(string character, string channel);
        bool IsUserMaster(string character);
        bool IsUserOP(string character, string channel);
        void JoinChannel(string channel);
        void KickUser(string character, string channel);
        void SendMessage(string message, string channel);
    }
}