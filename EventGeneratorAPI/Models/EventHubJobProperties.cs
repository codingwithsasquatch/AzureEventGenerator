using Newtonsoft.Json;

namespace EventGeneratorAPI.Models
{
    public class EventHubJobProperties : JobProperties
    {
        [JsonProperty("connectionString")]
        public string ConnectionString { get; set; }
        [JsonProperty("eventhub")]
        public string EventHub { get; set; }
    }
}
