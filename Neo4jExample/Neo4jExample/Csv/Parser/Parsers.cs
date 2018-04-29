// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Neo4jExample.Csv.Mapper;
using Neo4jExample.Csv.Model;
using TinyCsvParser;

namespace Neo4jExample.Csv.Parser
{
    public static class Parsers
    {
        public static CsvParser<Flight> FlightStatisticsParser
        {
            get
            {
                CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');

                return new CsvParser<Flight>(csvParserOptions, new FlightMapper());
            }
        }

        public static CsvParser<Airport> AirportParser
        {
            get
            {
                CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');

                return new CsvParser<Airport>(csvParserOptions, new AirportMapper());
            }
        }
    }
}