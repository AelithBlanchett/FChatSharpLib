﻿using Newtonsoft.Json;
using System;

namespace FChatSharpLib.Entities.Events.Helpers
{
    public class CharacterState
    {
        [JsonProperty("identity")]
        public string Character { get; set; }
        public GenderEnum Gender { get; set; }
        public string StatusText { get; set; }
        public StatusEnum Status { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
