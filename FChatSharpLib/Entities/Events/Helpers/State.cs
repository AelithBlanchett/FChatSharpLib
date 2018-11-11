using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FChatSharpLib.Entities.Events.Helpers
{
    public class State
    {
        public List<ChannelState> ChannelsInfo { get; set; } = new List<ChannelState>();
        public List<CharacterState> CharactersInfos { get; set; } = new List<CharacterState>();

        public void AddCharacterInChannel(string channel, string character)
        {
            if(!ChannelsInfo.Any(x => x.Channel == channel))
            {
                ChannelsInfo.Add(new ChannelState()
                {
                    Channel = channel
                });
            }

            if (!CharactersInfos.Any(x => x.Character == character))
            {
                CharactersInfos.Add(new CharacterState()
                {
                    Character = character
                });
            }

            var chanInfo = ChannelsInfo.First(x => x.Channel == channel);
            var charInfo = chanInfo.CharactersInfo.Find(x => x.Character == character);
            if(charInfo == null)
            {
                chanInfo.CharactersInfo.Add(CharactersInfos.First(x => x.Character == character));
            }
        }

        public void RemoveCharacterInChannel(string channel, string character)
        {
            var chanInfo = ChannelsInfo.FirstOrDefault(x => x.Channel == channel);
            if (chanInfo != null && chanInfo.CharactersInfo.Any(x => x.Character == character))
            {
                chanInfo.CharactersInfo.RemoveAll(x => x.Character == character);
            }
        }
    }
}
