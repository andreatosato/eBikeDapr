using eBike.Workers.Processing;
using eBike.Workers.Processing.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace eBike.Worker.Cron.Processing.Controllers
{
    [Route("scheduleJob")]
    [ApiController]
    public class ScheduleJobsController : ControllerBase
    {
        private readonly ILogger<ScheduleJobsController> logger;
        private readonly AggregateBikesCalculator aggregateBikesCalculator;
        private readonly IMongoCollection<BikeEntity> bikeEntityCollection;

        public ScheduleJobsController (ILogger<ScheduleJobsController> logger,
            AggregateBikesCalculator aggregateBikesCalculator,
            IMongoCollection<BikeEntity> bikeEntityCollection)
        {
            this.logger = logger;
            this.aggregateBikesCalculator = aggregateBikesCalculator;
            this.bikeEntityCollection = bikeEntityCollection;
        }

        [HttpPost]
        public async Task ScheduleCronAsync ()
        {
            logger.LogInformation("{0} ScheduleCronAsync", DateTime.Now);

            //Startup
            var bikesCount = await bikeEntityCollection.CountDocumentsAsync(FilterDefinition<BikeEntity>.Empty);
            if (bikesCount == 0) {
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
            }
            //Startup

            await aggregateBikesCalculator.ExecuteAsync();
        }
    }
}
