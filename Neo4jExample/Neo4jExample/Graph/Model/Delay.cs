// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Neo4jExample.Graph.Model
{
    public class Delay
    {
        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }
    }
}
