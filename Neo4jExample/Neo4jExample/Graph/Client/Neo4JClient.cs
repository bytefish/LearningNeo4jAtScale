// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver.V1;
using Neo4jExample.Core.Neo4j.Serializer;
using Neo4jExample.Core.Neo4j.Settings;
using Neo4jExample.Graph.Model;

namespace Neo4jExample
{
    public class Neo4JClient : IDisposable
    {
        private readonly IDriver driver;
        
        public Neo4JClient(IConnectionSettings settings)
        {
            this.driver = GraphDatabase.Driver(settings.Uri, settings.AuthToken);
        }

        public async Task CreateIndices()
        {
            string[] queries = {
                "CREATE CONSTRAINT ON (a:Airport) ASSERT a.id IS UNIQUE",
                "CREATE CONSTRAINT ON (c:City) ASSERT c.name IS UNIQUE",
                "CREATE CONSTRAINT ON (c:Country) ASSERT c.name IS UNIQUE",
                "CREATE CONSTRAINT ON (c:Carrier) ASSERT c.code IS UNIQUE",
                "CREATE INDEX ON :Flight(flight_number)",
                "CREATE INDEX ON :Airport(id)",
                "CREATE INDEX ON :Reason(code)",
                "CREATE INDEX ON :Carrier(code)",
            };

            using (var session = driver.Session())
            {
                foreach(var query in queries)
                {
                    await session.RunAsync(query);
                }
            }
        }

        public async Task CreateReasons(IList<Reason> reasons)
        {
            string cypher = new StringBuilder()
                .AppendLine("UNWIND {reasons} AS reason")
                .AppendLine("MERGE (r:Reason {code: reason.code})")
                .AppendLine("SET r = reason")
                .ToString();

            using (var session = driver.Session())
            {
                await session.RunAsync(cypher, new Dictionary<string, object>() { { "reasons", ParameterSerializer.ToDictionary(reasons) } });
            }
        }

        private async Task CreateCarriers(IList<Carrier> carriers)
        {
            string cypher = new StringBuilder()
                .AppendLine("UNWIND {carriers} AS carrier")
                .AppendLine("MERGE (c:Carrier {id: carrier.code})")
                .AppendLine("SET c = carrier")
                .ToString();

            using (var session = driver.Session())
            {
                await session.RunAsync(cypher, new Dictionary<string, object>() { { "carriers", ParameterSerializer.ToDictionary(carriers) } });
            }
        }

        private async Task CreateAirports(IList<AirportInformation> airports)
        {
            string cypher = new StringBuilder()
                .AppendLine("UNWIND {airports} AS row")
                // Add the Country:
                .AppendLine("MERGE (country:Country { name: row.country.name })")
                .AppendLine("SET country = row.country")
                .AppendLine()
                // Add the City:
                .AppendLine("WITH country")
                .AppendLine("MERGE (city:City { name: row.city.name })")
                .AppendLine("SET city = row.city")
                .AppendLine("MERGE (city)-[:COUNTRY]->(country)")
                .AppendLine()
                // Add the Airport:
                .AppendLine("WITH city")
                .AppendLine("MERGE (airport:Airport {id: row.airport.airport_id})")
                .AppendLine("SET a airport = row.airport")
                .AppendLine("MERGE (a)-[r:IN_CITY]->(c)")
                .AppendLine()
                .ToString();

            using (var session = driver.Session())
            {
                await session.RunAsync(cypher, new Dictionary<string, object>() { { "airports", ParameterSerializer.ToDictionary(airports) } });
            }
        }
        
        public async Task CreateFlights(IList<FlightInformation> flights)
        {
            string cypher = new StringBuilder()
                .AppendLine("UNWIND {flights} AS flight")
                // Get the Airports of this Flight:
                .AppendLine("MATCH (oAirport:Airport {airport_id: flight.origin}")
                .AppendLine("MATCH (dAirport {airport_id: flight.destination}")
                // Create the Flight Item:
                .AppendLine("CREATE (f:Flight {flight_num: flight.flight_number}), ")
                // Now add the ORIGIN and DESTINATION Relationships:
                .AppendLine("   (f)-[:ORIGIN {taxi_time: flight.taxi_out, dep_delay: flight.departure_delay},")
                .AppendLine("   (f)-[:DESTINATION {taxi_time: flight.taxi_in, arr_delay: flight.arrival_delay}")
                // Set Flight Information:
                .AppendLine("SET f.year = flight.year,")
                .AppendLine("    f.month = flight.month,")
                .AppendLine("    f.day = flight.day_of_month,")
                .AppendLine("    f.weekday = flight.day_of_week")
                .AppendLine()
                // Add Carrier Information:
                .AppendLine("WITH flight, f")
                .AppendLine("MATCH (car:Carrier {code: flight.carrier})")
                .AppendLine("MERGE (f)-[:CARRIER]->(car)")
                .AppendLine()
                // Add Delay Information:
                .AppendLine("WITH flight, f")
                .AppendLine("UNWIND flight.delays as delay")
                .AppendLine("   MATCH (r:Reason {code: delay.code}")
                .AppendLine("   MERGE (f)-[:DELAYED_BY {delay.duration}]->(r)")
                .AppendLine()
                // Add Cancellation Information:
                .AppendLine("WITH flight, f")
                .AppendLine("MATCH (r: Reason {name: flight.cancellation_code})")
                .AppendLine("MERGE(f)-[:CANCELLED_BY]->(r)")
                .ToString();


            using (var session = driver.Session())
            {
                await session.RunAsync(cypher, new Dictionary<string, object>() { { "flights", ParameterSerializer.ToDictionary(flights) } });
            }
        }

        public void Dispose()
        {
            driver?.Dispose();
        }
    }
}