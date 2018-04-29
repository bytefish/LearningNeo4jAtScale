// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Neo4jExample.Core.Neo4j.Settings;
using Neo4jExample.Csv.Parser;
using TinyCsvParser;

namespace Neo4jExample.ConsoleApp
{
    internal class Program
    {
        // File Names to read the Data from:
        private static readonly string carriersFileName = @"D:\github\LearningNeo4jAtScale\Resources\UNIQUE_CARRIERS.csv";
        private static readonly string airportsFileName = @"D:\github\LearningNeo4jAtScale\Resources\56803256_T_MASTER_CORD.csv";

        public static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
        }

        public static async Task RunAsync()
        {
            // Get the Base Data:
            var reasons = GetReasons();
            var carriers = GetCarriers(carriersFileName);
            var airports = GetAirportInformation(airportsFileName);
            
            var settings = ConnectionSettings.CreateBasicAuth("bolt://localhost:7687/db/actors", "neo4j", "test_pwd");

            using (var client = new Neo4JClient(settings))
            {
                // Create Indices for faster Lookups:
                await client.CreateIndices();

                // Create the base flight data:
                await client.CreateReasons(reasons);
                await client.CreateCarriers(carriers);
                await client.CreateAirports(airports);

                // Create Base Data:
                await client.CreateMovies(service.Movies);
            }
        }

        private static IList<Graph.Model.Carrier> GetCarriers(string filename)
        {
            return Parsers.CarrierParser
                .ReadFromFile(filename, Encoding.ASCII)
                .Where(x => x.IsValid)
                .Select(x => x.Result)
                .Select(x => new Graph.Model.Carrier
                {
                    Code = x.Code,
                    Description = x.Description
                })
                .ToList();
        }

        private static IList<Graph.Model.AirportInformation> GetAirportInformation(string filename)
        {
            return Parsers.AirportParser
                // Read from the Master Coordinate CSV File:
                .ReadFromFile(filename, Encoding.ASCII)
                // Only take valid entities:
                .Where(x => x.IsValid)
                // Get the parsed result:
                .Select(x => x.Result)
                // Only select the latest available data:
                .Where(x => x.AirportIsLatest)
                // Build the intermediate Airport Information:
                .Select(x => new Graph.Model.AirportInformation()
                {
                    Airport = new Graph.Model.Airport
                    {
                        AirportId = x.AirportId,
                        City = x.AirportCityName,
                        Country = x.AirportCountryName,
                        AirportIsLatest = x.AirportIsLatest,
                        AirportWac = x.AirportWac,
                        CountryCodeISO = x.AirportCountryCodeISO,
                        Name = x.AirportName,
                        State = x.AirportStateName
                    },
                    City = new Graph.Model.City
                    {
                        Name = x.AirportCityName
                    },
                    Country = new Graph.Model.Country()
                    {
                        Name = x.AirportCountryName,
                        IsoCode = x.AirportCountryCodeISO
                    }
                })
                // Evaluate:
                .ToList();
        }

        private static IList<Graph.Model.Reason> GetReasons()
        {
            return new[]
            {
                new Graph.Model.Reason
                {
                    Code = "A",
                    Description = "Carrier"
                },
                new Graph.Model.Reason()
                {
                    Code = "B",
                    Description = "Weather"
                },
                new Graph.Model.Reason()
                {
                    Code = "C",
                    Description = "National Air System"
                },
                new Graph.Model.Reason()
                {
                    Code = "D",
                    Description = "Security"
                },
                new Graph.Model.Reason()
                {
                    Code = "Z",
                    Description = "Late Aircraft"
                }
            };
        }

        private IList<Graph.Model.FlightInformation> GetFlightInformation(string filename)
        {
            return Parsers.FlightStatisticsParser
                // Read from the Master Coordinate CSV File:
                .ReadFromFile(filename, Encoding.ASCII)
                // Only take valid entities:
                .Where(x => x.IsValid)
                // Get the parsed result:
                .Select(x => x.Result)

                .Select(x => new Graph.Model.FlightInformation()
                {
                    Year = x.Year,
                    Month = x.Month,
                    DayOfMonth = x.DayOfMonth,
                    DayOfWeek = x.DayOfWeek,
                    Carrier = x.UniqueCarrier,
                    DestinationAirport = x.DestinationAirport,
                    OriginAirport = x.OriginAirport,
                    TailNumber = x.TailNumber,
                    Delays = GetDelays(x),
                    ArrivalDelay = x.ArrivalDelay,
                    CancellationCode = x.CancellationCode,
                    DepartureDelay = x.DepartureDelay,
                    FlightNumber = x.FlightNumber,
                    TaxiIn = x.TaxiIn,
                    TaxiOut = x.TaxiOut,
                })
                .ToList();
        }



        private IList<Graph.Model.Delay> GetDelays(Csv.Model.Flight source)
        {
            if (source == null)
            {
                return null;
            }

            var delays = new List<Graph.Model.Delay>();

            if (source.CarrierDelay.HasValue)
            {
                delays.Add(new Graph.Model.Delay {Duration = source.CarrierDelay.Value, Reason = "A"});
            }

            if (source.WeatherDelay.HasValue)
            {
                delays.Add(new Graph.Model.Delay {Duration = source.WeatherDelay.Value, Reason = "B"});
            }

            if (source.NasDelay.HasValue)
            {
                delays.Add(new Graph.Model.Delay {Duration = source.NasDelay.Value, Reason = "C"});
            }

            if (source.SecurityDelay.HasValue)
            {
                delays.Add(new Graph.Model.Delay {Duration = source.SecurityDelay.Value, Reason = "D"});
            }

            if (source.LateAircraftDelay.HasValue)
            {
                delays.Add(new Graph.Model.Delay {Duration = source.LateAircraftDelay.Value, Reason = "Z"});
            }

            return delays;
        }
    }
}