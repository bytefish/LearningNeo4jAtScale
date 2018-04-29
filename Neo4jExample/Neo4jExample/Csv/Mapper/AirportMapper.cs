// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Neo4jExample.Csv.Model;
using TinyCsvParser.Mapping;

namespace Neo4jExample.Csv.Mapper
{
    public class AirportMapper : CsvMapping<Airport>
    {
        public AirportMapper()
        {
            MapProperty(0, x => x.AirportId);
            MapProperty(1, x => x.AirportName);
            MapProperty(2, x => x.AirportCityName);
            MapProperty(3, x => x.AirportWac);
            MapProperty(4, x => x.AirportCountryName);
            MapProperty(5, x => x.AirportCountryCodeISO);
            MapProperty(6, x => x.AirportStateName);
            MapProperty(7, x => x.AirportIsLatest);
        }
    }
}
