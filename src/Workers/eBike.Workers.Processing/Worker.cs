using eBike.Workers.Processing.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace eBike.Workers.Processing
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly AggregateBikesCalculator aggregateBikesCalculator;
        private readonly IMongoCollection<BikeEntity> bikeEntityCollection;

        public Worker (ILogger<Worker> logger, AggregateBikesCalculator aggregateBikesCalculator, IMongoCollection<BikeEntity> bikeEntityCollection)
        {
            _logger = logger;
            this.aggregateBikesCalculator = aggregateBikesCalculator;
            this.bikeEntityCollection = bikeEntityCollection;
        }

        protected override async Task ExecuteAsync (CancellationToken stoppingToken)
        {
            //Startup
            var b1 = new BikeEntity { Id = Guid.NewGuid(), Latitude = 51.507222, Longitude = -0.1275 };
            var b2 = new BikeEntity { Id = Guid.NewGuid(), Latitude = 45.495759, Longitude = 9.1794273 };
            var b3 = new BikeEntity { Id = Guid.NewGuid(), Latitude = 43.779936, Longitude = 11.170928 };
            var b4 = new BikeEntity { Id = Guid.NewGuid(), Latitude = 41.902803, Longitude = 12.4532382 };
            var b5 = new BikeEntity { Id = Guid.NewGuid(), Latitude = 40.407668, Longitude = -3.604443 };
            var b6 = new BikeEntity { Id = Guid.NewGuid(), Latitude = 41.394768, Longitude = 2.0787279 };
            var b7 = new BikeEntity { Id = Guid.NewGuid(), Latitude = 39.407701, Longitude = -0.5015956 };

            var bikes = new[] { b1, b2, b3, b4, b5, b6, b7 };
            foreach (var bItem in bikes) {
                await bikeEntityCollection.FindOneAndReplaceAsync<BikeEntity>(t =>
                    t.Latitude == bItem.Latitude && t.Longitude == bItem.Longitude,
                    bItem,
                    new FindOneAndReplaceOptions<BikeEntity, BikeEntity>() { IsUpsert = true });
            }
            //Startup




            while (!stoppingToken.IsCancellationRequested) {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                try {
                    await aggregateBikesCalculator.ExecuteAsync();
                    await Task.Delay((int)System.TimeSpan.FromMinutes(10).TotalMilliseconds, stoppingToken);
                }
                catch (OperationCanceledException) {
                    return;
                }
            }
        }
    }
}
