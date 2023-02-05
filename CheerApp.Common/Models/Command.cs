using System;
using System.Text.Json.Serialization;

namespace CheerApp.Common
{
    public class Command
    {
        public Command()
        {
        }

        [JsonPropertyName("FE")]
        public FeatureEnum Feature { get; set; }

        [JsonPropertyName("CD")]
        public string CommandDetails { get; set; }

        [JsonPropertyName("CT")]
        public int CommandTime { get; set; }
    }
}