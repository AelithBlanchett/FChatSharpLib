using System.Collections.Generic;

namespace FChatSharpLib.Entities.Events.Helpers
{
    public class ChannelState
    {
        public string Channel { get; set; }
        public List<CharacterState> CharactersInfo { get; set; } = new List<CharacterState>();
    }
}
