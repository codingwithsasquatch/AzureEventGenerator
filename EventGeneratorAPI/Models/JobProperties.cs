using Newtonsoft.Json;

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
    }
}
