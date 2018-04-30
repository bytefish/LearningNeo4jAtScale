using Newtonsoft.Json;

namespace Neo4jExample.Graph.Model
{
    public class State
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
