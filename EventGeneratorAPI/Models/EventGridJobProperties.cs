using Newtonsoft.Json;

namespace EventGeneratorAPI.Models
{
    public class EventGridJobProperties : JobProperties
    {
        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
