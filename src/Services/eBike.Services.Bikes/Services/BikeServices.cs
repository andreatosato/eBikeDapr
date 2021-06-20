using Dapr.Client;
using eBike.Commons.Events;
using eBike.Services.Bikes.Entities;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using static eBike.Services.Bikes.BikeEndpoint;

namespace eBike.Services.Bikes.Services
{
    public class BikeServices : BikeEndpointBase
    {
        private readonly ILogger<BikeServices> _logger;
        private readonly DaprClient daprClient;
        private readonly IMongoCollection<BikeEntity> bikeCollection;

        public BikeServices (ILogger<BikeServices> logger, DaprClient daprClient, IMongoCollection<BikeEntity> bikeCollection)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            this.daprClient = daprClient ?? throw new System.ArgumentNullException(nameof(daprClient));
            this.bikeCollection = bikeCollection ?? throw new System.ArgumentNullException(nameof(bikeCollection));
        }

        public override async Task<BikeReply> CreateOrUpdate (BikeRequest request, ServerCallContext context)
        {
            var bikeId = Guid.Parse(request.BikeId);

            var exists = await bikeCollection.CountDocumentsAsync(t => t.Id == bikeId);

            var bikeCreated = await bikeCollection.FindOneAndReplaceAsync(
                t => t.Id == bikeId,
                new BikeEntity {
                    Id = bikeId,
                    Name = request.Name,
                    UserId = Guid.Parse(request.UserId),
                    Latitude = request.Latitude,
                    Longitude = request.Longitude
                });

            await daprClient.PublishEventAsync(
                Environment.GetEnvironmentVariable("PUBSUB_NAME"),
                "bike",
                new BikeEvent {
                    Id = bikeCreated.Id,
                    Name = bikeCreated.Name,
                    UserId = bikeCreated.UserId,
                    Latitude = bikeCreated.Latitude,
                    Longitude = bikeCreated.Longitude,
                    Status = exists == 0 ? BikeStatus.Created : BikeStatus.Updated
                });

            return new BikeReply() {
                BikeId = bikeCreated.Id.ToString(),
                OperationResult = exists == 0 ? Operation.Create : Operation.Update
            };
        }
    }
}
