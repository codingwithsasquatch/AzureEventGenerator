using Newtonsoft.Json;
using System.Collections.Generic;

namespace EventGeneratorAPI.Models
{
    public class JobProperties
    {
        [JsonProperty("frequency")]
        public int Frequency { get; set; }
        [JsonProperty("duration")]
        public int Duration { get; set; }
        [JsonProperty("messageScheme")]
        public string MessageScheme { get; set; }
        [JsonProperty("messageMethod")]
        public string MessageMethod { get; set; }
        public IEnumerable<string> Messages {get; set; }
    }
}
