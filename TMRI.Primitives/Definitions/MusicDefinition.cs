﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TMRI.Primitives.Definitions
{
    public class MusicDefinition
    {
        public ProductInfo Product { get; set; }
        public UpdateInfo Update { get; set; }
        public List<TrackInfo> Playlist { get; set; }
        [JsonIgnore]
        public string Path { get; set; }
    }
}
