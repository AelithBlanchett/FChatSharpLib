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
        public bool IsBotReady { get; set; } = false;

        public List<CharacterState> GetAllCharactersInChannel(string channel)
        {
            var hasChanInfo = ChannelsInfo.TryGetValue(channel, out var chanInfo);
            if(hasChanInfo)
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
                    Character = character,
                    LastUpdate = DateTime.UtcNow
                });
            }

            var hasChanInfo = ChannelsInfo.TryGetValue(channel, out var chanInfo);
            
            if(hasChanInfo)
            {
                var charInfo = chanInfo.CharactersInfo.Find(x => x.Character.ToLower() == character.ToLower());
                if(charInfo == null)
                {
                    var hasFoundCharacter = CharactersInfos.TryGetValue(character, out var actualCharacterInfo);
                    chanInfo.CharactersInfo.Add(actualCharacterInfo);
                }
            }
        }

        public void RemoveCharacterInChannel(string channel, string character)
        {
            var hasChanInfo = ChannelsInfo.TryGetValue(channel, out var chanInfo);
            if (hasChanInfo && chanInfo.CharactersInfo.Any(x => x.Character.ToLower() == character.ToLower()))
            {
                chanInfo.CharactersInfo.RemoveAll(x => x.Character.ToLower() == character.ToLower());
            }
        }

        private readonly object balanceLock = new object();

        public string Serialize()
        {
            lock (balanceLock)
            {
                return JsonConvert.SerializeObject(this);
            }
        }
    }
}
