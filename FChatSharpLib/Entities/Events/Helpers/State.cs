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

        public string BotCharacterName { get; set; }
        public string AdminCharacterName { get; set; }

        public List<CharacterState> GetAllCharactersInChannel(string channel)
        {
            var chanInfo = ChannelsInfo.FirstOrDefault(x => x.Channel.ToLower() == channel.ToLower());
            if(chanInfo != null)
            {
                return chanInfo.CharactersInfo;
            }
            else
            {
                return new List<CharacterState>();
            }
        }

        public IEnumerable<string> Channels
        {
            get
            {
                return ChannelsInfo.Select(x => x.Channel);
            }
        }

        public void AddCharacterInChannel(string channel, string character)
        {
            if(!ChannelsInfo.Any(x => x.Channel.ToLower() == channel.ToLower()))
            {
                ChannelsInfo.Add(new ChannelState()
                {
                    Channel = channel.ToLower()
                });
            }

            if (!CharactersInfos.Any(x => x.Character.ToLower() == character.ToLower()))
            {
                CharactersInfos.Add(new CharacterState()
                {
                    Character = character.ToLower()
                });
            }

            var chanInfo = ChannelsInfo.First(x => x.Channel.ToLower() == channel.ToLower());
            var charInfo = chanInfo.CharactersInfo.Find(x => x.Character.ToLower() == character.ToLower());
            if(charInfo == null)
            {
                chanInfo.CharactersInfo.Add(CharactersInfos.First(x => x.Character.ToLower() == character.ToLower()));
            }
        }

        public void RemoveCharacterInChannel(string channel, string character)
        {
            var chanInfo = ChannelsInfo.FirstOrDefault(x => x.Channel.ToLower() == channel.ToLower());
            if (chanInfo != null && chanInfo.CharactersInfo.Any(x => x.Character.ToLower() == character.ToLower()))
            {
                chanInfo.CharactersInfo.RemoveAll(x => x.Character.ToLower() == character.ToLower());
            }
        }
    }
}
