using Newtonsoft.Json;

namespace EventGeneratorAPI.Models
{
    public class StorageQueueJobProperties : JobProperties
    {
        [JsonProperty("connectionString")]
        public string ConnectionString { get; set; }
        [JsonProperty("queue")]
        public string Queue { get; set; }
    }
}
