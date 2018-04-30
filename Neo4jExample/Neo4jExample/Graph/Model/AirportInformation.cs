// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using Newtonsoft.Json;

namespace Neo4jExample.Graph.Model
{
    public class AirportInformation
    {
        [JsonProperty("airport")]
        public Airport Airport { get; set; }

        [JsonProperty("city")]
        public City City { get; set; }

        [JsonProperty("country")]
        public Country Country { get; set; }

        [JsonProperty("state")]
        public State State { get; set; }
    }
}
