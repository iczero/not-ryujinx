﻿using System;
using System.Text.Json.Serialization;

namespace Ryujinx.Ui.App.Common
{
    public class ApplicationMetadata
    {
        public string Title { get; set; }
        public bool   Favorite   { get; set; }

        [JsonPropertyName("timespan_played")]
        public TimeSpan? TimePlayed { get; set; }

        [JsonPropertyName("last_played_utc")]
        public DateTime? LastPlayed { get; set; } = null;

        [JsonPropertyName("time_played")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double TimePlayedOld { get; set; }

        [JsonPropertyName("last_played")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string LastPlayedOld { get; set; }
    }
}