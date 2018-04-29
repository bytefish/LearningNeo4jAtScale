// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Neo4jExample.Graph.Model
{
    public class CancelledFlightInformation
    {
        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("month")]
        public int Month { get; set; }

        [JsonProperty("day_of_month")]
        public int DayOfMonth { get; set; }

        [JsonProperty("day_of_week")]
        public string DayOfWeek { get; set; }

        [JsonProperty("carrier")]
        public string Carrier { get; set; }

        [JsonProperty("tail_number")]
        public string TailNumber { get; set; }

        [JsonProperty("origin")]
        public string OriginAirport { get; set; }

        [JsonProperty("destination")]
        public string DestinationAirport { get; set; }

        [JsonProperty("cancellation_reason")]
        public string CancellationReason { get; set; }
    }
}
