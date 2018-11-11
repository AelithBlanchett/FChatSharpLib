namespace FChatSharpLib.Entities.Events
{
    public interface IBaseFChatEvent
    {
        string Data { get; }
        string Type { get; set; }

        string ToString();
    }
}