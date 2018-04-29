// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Neo4j.Driver.V1;
using Newtonsoft.Json;

namespace Neo4jExample.Graph.Model
{
    public class FlightInformation
    {
        [JsonProperty("flight_number")]
        public string FlightNumber { get; set; }

        [JsonProperty("tail_number")]
        public string TailNumber { get; set; }

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
        public int DayOfWeek { get; set; }

        [JsonProperty("delays")]
        public IList<Delay> Delays { get; set; }

        [JsonProperty("taxi_out")]
        public int? TaxiOut { get; set; }

        [JsonProperty("departure_delay")]
        public int? DepartureDelay { get; set; }

        [JsonProperty("taxi_in")]
        public int? TaxiIn { get; set; }

        [JsonProperty("arrival_delay")]
        public int? ArrivalDelay { get; set; }

        [JsonProperty("cancellation_code")]
        public string CancellationCode { get; set; }
    }
}