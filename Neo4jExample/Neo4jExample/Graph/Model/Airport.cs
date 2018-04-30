// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Neo4jExample.Graph.Model
{
    public class Airport
    {
        [JsonProperty("airport_id")]
        public string AirportId { get; set; }

        [JsonProperty("abbr")]
        public string Abbr { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
        
        [JsonProperty("country")]
        public string Country { get; set; }
        
        [JsonProperty("wac")]
        public string AirportWac { get; set; }

        [JsonProperty("country_code_iso")]
        public string CountryCodeISO { get; set; }
        
        [JsonProperty("is_latest")]
        public bool AirportIsLatest { get; set; }
    }
}
