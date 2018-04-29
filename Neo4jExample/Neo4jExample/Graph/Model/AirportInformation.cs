// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace Neo4jExample.Graph.Model
{
    public class AirportInformation
    {
        public Airport Airport { get; set; }

        public City City { get; set; }

        public Country Country { get; set; }
    }
}
