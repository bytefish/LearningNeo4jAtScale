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
                "CREATE INDEX ON :City(name)",
                "CREATE INDEX ON :Country(name)",
                "CREATE INDEX ON :Carrier(code)",
                "CREATE INDEX ON :Airport(id)",
                "CREATE INDEX ON :Reason(code)",
            };

            using (var session = driver.Session())
            {
                foreach(var query in queries)
                {
                    await session.RunAsync(query);
                }
            }
        }

        public async Task CreateCities(IList<City> cities)
        {
            string cypher = new StringBuilder()
                .AppendLine("UNWIND {cities} AS city")
                .AppendLine("MERGE (c:City {name: city.name})")
                .AppendLine("SET c = city")
                .ToString();

            using (var session = driver.Session())
            {
                await session.RunAsync(cypher, new Dictionary<string, object>() { { "cities", ParameterSerializer.ToDictionary(cities) } });
            }
        }

        public async Task CreateGenres(IList<Country> countries)
        {
            string cypher = new StringBuilder()
                .AppendLine("UNWIND {countries} AS country")
                .AppendLine("MERGE (c:Country {name: country.name})")
                .AppendLine("SET c = country")
                .ToString();

            using (var session = driver.Session())
            {
                await session.RunAsync(cypher, new Dictionary<string, object>() { { "countries", ParameterSerializer.ToDictionary(countries) } });
            }
        }

        public async Task CreateCarriers(IList<Carrier> carriers)
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

        public async Task CreateAirports(IList<Airport> airports)
        {
            string cypher = new StringBuilder()
                .AppendLine("UNWIND {airports} AS airport")
                .AppendLine("MERGE (a:Airport {id: airport.id})")
                .AppendLine("SET a = airport")
                .AppendLine("WITH a")
                .AppendLine("MATCH (c:City {name: a.city})")
                .AppendLine("MERGE (a)-[r:IN_CITY]->(c)")
                .ToString();

            using (var session = driver.Session())
            {
                await session.RunAsync(cypher, new Dictionary<string, object>() { { "airports", ParameterSerializer.ToDictionary(airports) } });
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

        public async Task CreateFlights(IList<FlightInformation> metadatas)
        {
            string cypher = new StringBuilder()
                .AppendLine("UNWIND {metadatas} AS metadata")
                .ToString();


            using (var session = driver.Session())
            {
                await session.RunAsync(cypher, new Dictionary<string, object>() { { "metadatas", ParameterSerializer.ToDictionary(metadatas) } });
            }
        }

        public async Task CreateCancelledFlights(IList<CancelledFlightInformation> metadatas)
        {
            string cypher = new StringBuilder()
                .AppendLine("UNWIND {metadatas} AS metadata")
                .ToString();


            using (var session = driver.Session())
            {
                await session.RunAsync(cypher, new Dictionary<string, object>() { { "metadatas", ParameterSerializer.ToDictionary(metadatas) } });
            }
        }


        public void Dispose()
        {
            driver?.Dispose();
        }
    }
}