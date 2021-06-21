using eBike.Workers.Processing.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System;

namespace eBike.Workers.Processing
{
    public class Program
    {
        public static void Main (string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder (string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) => {
                    services.AddHostedService<Worker>();
                    services.AddHttpClient("AzureMaps");
                    services.AddScoped((sp) => {
                        var client = new MongoClient(Environment.GetEnvironmentVariable("ConnectionString"));
                        return client.GetDatabase("Bikes").GetCollection<BikeEntity>("BikesCollection");
                    });
                    services.AddScoped((sp) => {
                        var client = new MongoClient(Environment.GetEnvironmentVariable("ConnectionString"));
                        return client.GetDatabase("Bikes").GetCollection<BikeAggregation>("BikesAggregateCollection");
                    });

                    services.AddScoped<AggregateBikesCalculator>();
                });
    }
}
