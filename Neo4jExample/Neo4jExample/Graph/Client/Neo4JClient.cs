﻿// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver.V1;
using Neo4jExample.Core.Linq;
using Neo4jExample.Core.Neo4j.Serializer;
using Neo4jExample.Core.Neo4j.Settings;
using Neo4jExample.Graph.Model;

namespace Neo4jExample.Graph.Client
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
                "CREATE CONSTRAINT ON (f:Flight) ASSERT f.flight_number IS UNIQUE",
                "CREATE CONSTRAINT ON (a:Airport) ASSERT a.airport_id IS UNIQUE",
                "CREATE CONSTRAINT ON (r:Reason) ASSERT r.code IS UNIQUE",
                "CREATE CONSTRAINT ON (c:City) ASSERT c.name IS UNIQUE",
                "CREATE CONSTRAINT ON (c:Country) ASSERT c.name IS UNIQUE",
                "CREATE CONSTRAINT ON (c:Carrier) ASSERT c.code IS UNIQUE"
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

        public async Task CreateCarriers(IList<Carrier> carriers)
        {
            string cypher = new StringBuilder()
                .AppendLine("UNWIND {carriers} AS carrier")
                .AppendLine("MERGE (c:Carrier {code: carrier.code})")
                .AppendLine("SET c = carrier")
                .ToString();

            using (var session = driver.Session())
            {
                await session.RunAsync(cypher, new Dictionary<string, object>() { { "carriers", ParameterSerializer.ToDictionary(carriers) } });
            }
        }

        public async Task CreateAirports(IList<AirportInformation> airports)
        {
            string cypher = new StringBuilder()
                .AppendLine("UNWIND {airports} AS row")
                // Add the Country:
                .AppendLine("MERGE (aCountry:Country { name: row.country.name })")
                .AppendLine("SET aCountry = row.country")
                .AppendLine()
                // Add the City:
                .AppendLine("WITH aCountry, row")
                .AppendLine("MERGE (aCity:City { name: row.city.name })")
                .AppendLine("SET aCity = row.city")
                .AppendLine("MERGE (aCity)-[:COUNTRY]->(aCountry)")
                .AppendLine()
                //// Add the Airport:
                .AppendLine("WITH aCity, row")
                .AppendLine("MERGE (airport:Airport {airport_id: row.airport.airport_id})")
                .AppendLine("SET airport = row.airport")
                .AppendLine("MERGE (a)-[r:IN_CITY]->(aCity)")
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
                .AppendLine("UNWIND {flights} AS row")
                // Get the Airports of this Flight:
                .AppendLine("MATCH (oAirport:Airport {airport_id: row.origin})")
                .AppendLine("MATCH (dAirport:Airport {airport_id: row.destination})")
                // Create the Flight Item:
                .AppendLine("MERGE (f:Flight {flight_number: row.flight_number})")
                // Set Flight Details:
                .AppendLine("SET f.year = row.year,")
                .AppendLine("    f.month = row.month,")
                .AppendLine("    f.day = row.day_of_month,")
                .AppendLine("    f.weekday = row.day_of_week")
                .AppendLine()
                // Relate Flight to Origin Airport:
                .AppendLine("MERGE (f)-[o:ORIGIN]->(oAirport)")
                .AppendLine("SET o.taxi_time = row.taxi_out,")
                .AppendLine("    o.dep_delay = row.departure_delay")
                .AppendLine()
                // Relate Flight to Destination Airport:
                .AppendLine("MERGE (f)-[d:DESTINATION]->(dAirport)")
                .AppendLine("SET d.taxi_time = row.taxi_in,")
                .AppendLine("    d.arr_delay = row.arrival_delay")
                .AppendLine()
                // Add Carrier Information:
                .AppendLine("WITH row, f")
                .AppendLine("MATCH (car:Carrier {code: row.carrier})")
                .AppendLine("MERGE (f)-[:CARRIER]->(car)")
                .AppendLine()
                // Add Cancellation Information:
                .AppendLine("WITH row, f")
                .AppendLine("OPTIONAL MATCH (r:Reason {code: row.cancellation_code})")
                .AppendLine("FOREACH (o IN CASE WHEN r IS NOT NULL THEN [r] ELSE [] END |")
                .AppendLine("   MERGE(f) -[:CANCELLED_BY]->(r)")
                .AppendLine(")")
                .AppendLine()
                // Add Delay Information:
                .AppendLine("WITH row, f")
                .AppendLine("UNWIND row.delays as delay")
                .AppendLine("OPTIONAL MATCH (r:Reason {code: delay.reason})")
                .AppendLine("FOREACH (o IN CASE WHEN r IS NOT NULL THEN [r] ELSE [] END |")
                .AppendLine("   MERGE (f)-[fd:DELAYED_BY]->(r)")
                .AppendLine("   SET fd.delay = delay.duration")
                .AppendLine(")")
                .AppendLine()
                .ToString();

            using (var session = driver.Session())
            {
                foreach (var batch in flights.Batch(10))
                {
                    await session.RunAsync(cypher, new Dictionary<string, object>() {{"flights", ParameterSerializer.ToDictionary(batch.ToList()) }});
                }
            }
        }

        public void Dispose()
        {
            driver?.Dispose();
        }
    }
}