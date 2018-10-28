namespace FChatSharpLib.Entities.Events
{
    public interface IBaseEvent
    {
        string Data { get; }
        string Type { get; set; }

        string ToString();
    }
}