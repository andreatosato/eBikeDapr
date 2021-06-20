using Dapr.AppCallback.Autogen.Grpc.v1;
using Dapr.Client;
using Dapr.Client.Autogen.Grpc.v1;
using eBike.Commons.Events;
using eBike.Services.Bikes.Entities;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace eBike.Services.Bikes.Services
{
    public class BikeServices : AppCallback.AppCallbackBase
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private async Task<BikeReply> CreateOrUpdate (BikeRequest request, ServerCallContext context)
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

        public override async Task<InvokeResponse> OnInvoke (InvokeRequest request, ServerCallContext context)
        {
            var response = new InvokeResponse();
            switch (request.Method) {
                case "CreateOrUpdate":
                    var input = request.Data.Unpack<BikeRequest>();
                    var output = await CreateOrUpdate(input, context);
                    response.Data = Any.Pack(output);
                    break;
            }
            return response;
        }

        public override Task<ListTopicSubscriptionsResponse> ListTopicSubscriptions (Empty request, ServerCallContext context)
        {
            return base.ListTopicSubscriptions(request, context);
        }

        public override Task<TopicEventResponse> OnTopicEvent (TopicEventRequest request, ServerCallContext context)
        {
            return base.OnTopicEvent(request, context);
        }
    }
}
