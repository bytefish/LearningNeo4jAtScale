// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Neo4jExample.Core.Neo4j.Settings;
using Neo4jExample.Csv.Parser;
using Neo4jExample.Graph.Client;
using TinyCsvParser;

namespace Neo4jExample.ConsoleApp
{
    internal class Program
    {
        // File Names to read the Data from:
        private static readonly string csvCarriersFile = @"D:\github\LearningNeo4jAtScale\Resources\UNIQUE_CARRIERS.csv";
        private static readonly string csvAirportsFile = @"D:\github\LearningNeo4jAtScale\Resources\56803256_T_MASTER_CORD.csv";

        // Use the 2014 Flight Data:
        private static readonly string[] csvFlightStatisticsFiles = new[]
        {
            "D:\\datasets\\AOTP\\ZIP\\AirOnTimeCSV_1987_2017\\AirOnTimeCSV\\airOT201401.csv",
            "D:\\datasets\\AOTP\\ZIP\\AirOnTimeCSV_1987_2017\\AirOnTimeCSV\\airOT201402.csv",
            "D:\\datasets\\AOTP\\ZIP\\AirOnTimeCSV_1987_2017\\AirOnTimeCSV\\airOT201403.csv",
            "D:\\datasets\\AOTP\\ZIP\\AirOnTimeCSV_1987_2017\\AirOnTimeCSV\\airOT201404.csv",
            "D:\\datasets\\AOTP\\ZIP\\AirOnTimeCSV_1987_2017\\AirOnTimeCSV\\airOT201405.csv",
            "D:\\datasets\\AOTP\\ZIP\\AirOnTimeCSV_1987_2017\\AirOnTimeCSV\\airOT201406.csv",
            "D:\\datasets\\AOTP\\ZIP\\AirOnTimeCSV_1987_2017\\AirOnTimeCSV\\airOT201407.csv",
            "D:\\datasets\\AOTP\\ZIP\\AirOnTimeCSV_1987_2017\\AirOnTimeCSV\\airOT201408.csv",
            "D:\\datasets\\AOTP\\ZIP\\AirOnTimeCSV_1987_2017\\AirOnTimeCSV\\airOT201409.csv",
            "D:\\datasets\\AOTP\\ZIP\\AirOnTimeCSV_1987_2017\\AirOnTimeCSV\\airOT201410.csv",
            "D:\\datasets\\AOTP\\ZIP\\AirOnTimeCSV_1987_2017\\AirOnTimeCSV\\airOT201411.csv",
            "D:\\datasets\\AOTP\\ZIP\\AirOnTimeCSV_1987_2017\\AirOnTimeCSV\\airOT201412.csv",
        };

        public static void Main(string[] args)
        {
            var settings = ConnectionSettings.CreateBasicAuth("bolt://localhost:7687/db/flights", "neo4j", "test_pwd");

            using (var client = new Neo4JClient(settings))
            {
                // Create Indices for faster Lookups:
                client.CreateIndices();
                // Insert the Base Data (Airports, Carriers, ...):
                InsertBaseData(client);
                // Insert the Flight Data:
                InsertFlightData(client);
            }
        }

        private static void InsertBaseData(Neo4JClient client)
        {
            // Create the base flight data:
            client.CreateReasons(GetReasons());

            Console.WriteLine($"Starting Carriers CSV Import: {csvCarriersFile}");

            GetCarriers(csvCarriersFile)
                // As an Observable:
                .ToObservable()
                // Batch in 1000 Entities / or wait 1 Second:
                .Buffer(TimeSpan.FromSeconds(1), 1000)
                // Insert when Buffered:
                .Subscribe(records =>
                {
                    client.CreateCarriers(records);
                });

            Console.WriteLine($"Starting Airports CSV Import: {csvAirportsFile}");

            GetAirportInformation(csvAirportsFile)
                // As an Observable:
                .ToObservable()
                // Batch in 1000 Entities / or wait 1 Second:
                .Buffer(TimeSpan.FromSeconds(1), 1000)
                // Insert when Buffered:
                .Subscribe(records =>
                {
                    client.CreateAirports(records);
                });
        }

        private static void InsertFlightData(Neo4JClient client)
        {
            // Create Flight Data with Batched Items:
            foreach (var csvFlightStatisticsFile in csvFlightStatisticsFiles)
            {
                Console.WriteLine($"Starting Flights CSV Import: {csvFlightStatisticsFile}");

                GetFlightInformation(csvFlightStatisticsFile)
                    // As an Observable:
                    .ToObservable()
                    // Batch in 1000 Entities / or wait 1 Second:
                    .Buffer(TimeSpan.FromSeconds(1), 1000)
                    // Insert when Buffered:
                    .Subscribe(records =>
                    {
                        client.CreateFlights(records);
                    });
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

        private static ParallelQuery<Graph.Model.AirportInformation> GetAirportInformation(string filename)
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
                            State = x.AirportStateName,
                            Abbr = x.AirportAbbr
                        },
                        City = new Graph.Model.City
                        {
                            Name = x.AirportCityName
                        },
                        Country = new Graph.Model.Country
                        {
                            Name = x.AirportCountryName,
                            IsoCode = x.AirportCountryCodeISO
                        },
                        State = new Graph.Model.State
                        {
                            Code = x.AirportStateCode,
                            Name = x.AirportStateName
                        }
                    });
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

        private static ParallelQuery<Graph.Model.FlightInformation> GetFlightInformation(string filename)
        {
            return Parsers.FlightStatisticsParser
                // Read from the Master Coordinate CSV File:
                .ReadFromFile(filename, Encoding.ASCII)
                // Only take valid entities:
                .Where(x => x.IsValid)
                // Get the parsed result:
                .Select(x => x.Result)
                // Return the Graph Model:
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
                });
        }

        private static IList<Graph.Model.Delay> GetDelays(Csv.Model.Flight source)
        {
            if (source == null)
            {
                return null;
            }

            var delays = new List<Graph.Model.Delay>();

            if (source.CarrierDelay.HasValue && source.CarrierDelay > 0)
            {
                delays.Add(new Graph.Model.Delay { Duration = source.CarrierDelay.Value, Reason = "A" });
            }

            if (source.WeatherDelay.HasValue && source.WeatherDelay > 0)
            {
                delays.Add(new Graph.Model.Delay { Duration = source.WeatherDelay.Value, Reason = "B" });
            }

            if (source.NasDelay.HasValue && source.NasDelay > 0)
            {
                delays.Add(new Graph.Model.Delay { Duration = source.NasDelay.Value, Reason = "C" });
            }

            if (source.SecurityDelay.HasValue && source.SecurityDelay > 0)
            {
                delays.Add(new Graph.Model.Delay { Duration = source.SecurityDelay.Value, Reason = "D" });
            }

            if (source.LateAircraftDelay.HasValue && source.LateAircraftDelay > 0)
            {
                delays.Add(new Graph.Model.Delay { Duration = source.LateAircraftDelay.Value, Reason = "Z" });
            }

            return delays;
        }
    }
}