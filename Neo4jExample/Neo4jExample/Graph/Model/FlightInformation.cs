// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Neo4jExample.Graph.Model
{
    public class FlightInformation
    {
        [JsonProperty("carrier")]
        public string Carrier { get; set; }

        [JsonProperty("origin")]
        public string OriginAirport { get; set; }

        [JsonProperty("destination")]
        public string DestinationAirport { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("month")]
        public int Month { get; set; }

        [JsonProperty("dayOfMonth")]
        public int DayOfMonth { get; set; }

        [JsonProperty("dayOfWeek")]
        public string DayOfWeek { get; set; }

        [JsonProperty("delays")]
        public IList<Delay> Delays { get; set; }
    }
}