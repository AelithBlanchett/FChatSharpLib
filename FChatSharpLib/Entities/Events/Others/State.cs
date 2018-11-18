using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FChatSharpLib.Entities.Events.Helpers
{
    [Serializable]
    public class State
    {
        public ConcurrentDictionary<string, ChannelState> ChannelsInfo { get; set; } = new ConcurrentDictionary<string, ChannelState>(StringComparer.OrdinalIgnoreCase);
        public ConcurrentDictionary<string, CharacterState> CharactersInfos { get; set; } = new ConcurrentDictionary<string, CharacterState>(StringComparer.OrdinalIgnoreCase);

        public string BotCharacterName { get; set; }
        public string AdminCharacterName { get; set; }
        public bool IsBotReady { get; set; }


        public List<CharacterState> GetAllCharactersInChannel(string channel)
        {
            var chanInfo = ChannelsInfo.GetValueOrDefault(channel);
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
                return ChannelsInfo.Select(x => x.Key);
            }
        }

        public void AddCharacterInChannel(string channel, string character)
        {
            if(!ChannelsInfo.ContainsKey(channel))
            {
                ChannelsInfo.TryAdd(channel, new ChannelState()
                {
                    Channel = channel
                });
            }

            if (!CharactersInfos.ContainsKey(character))
            {
                CharactersInfos.TryAdd(character, new CharacterState()
                {
                    Character = character
                });
            }

            var chanInfo = ChannelsInfo.GetValueOrDefault(channel);
            var charInfo = chanInfo.CharactersInfo.Find(x => x.Character.ToLower() == character.ToLower());
            if(charInfo == null)
            {
                chanInfo.CharactersInfo.Add(CharactersInfos.GetValueOrDefault(character));
            }
        }

        public void RemoveCharacterInChannel(string channel, string character)
        {
            var chanInfo = ChannelsInfo.GetValueOrDefault(channel);
            if (chanInfo != null && chanInfo.CharactersInfo.Any(x => x.Character.ToLower() == character.ToLower()))
            {
                chanInfo.CharactersInfo.RemoveAll(x => x.Character.ToLower() == character.ToLower());
            }
        }

        public string Serialize()
        {
            lock (this)
            {
                return JsonConvert.SerializeObject(this);
            }
        }
    }
}
