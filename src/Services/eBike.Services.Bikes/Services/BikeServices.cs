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
        private readonly IMongoCollection<BikeAggregation> bikeAggregationCollection;

        public BikeServices (ILogger<BikeServices> logger,
            DaprClient daprClient,
            IMongoCollection<BikeEntity> bikeCollection,
            IMongoCollection<BikeAggregation> bikeAggregationCollection)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            this.daprClient = daprClient ?? throw new System.ArgumentNullException(nameof(daprClient));
            this.bikeCollection = bikeCollection ?? throw new System.ArgumentNullException(nameof(bikeCollection));
            this.bikeAggregationCollection = bikeAggregationCollection ?? throw new ArgumentNullException(nameof(bikeAggregationCollection));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Mantain Standard Behaviour")]
        private async Task<BikeReply> CreateOrUpdate (BikeRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.BikeId, out var bikeId))
                bikeId = Guid.NewGuid();

            var exists = await bikeCollection.CountDocumentsAsync(t => t.Id == bikeId);

            var options = new FindOneAndReplaceOptions<BikeEntity>() {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true
            };
            var bikeCreated = await bikeCollection.FindOneAndReplaceAsync<BikeEntity>(
                t => t.Id == bikeId,
                new BikeEntity {
                    Id = bikeId,
                    Name = request.Name,
                    UserId = Guid.Parse(request.UserId),
                    Latitude = request.Latitude,
                    Longitude = request.Longitude
                }, options);

            await daprClient.PublishEventAsync(
                Environment.GetEnvironmentVariable("PUBSUB_NAME"),
                "bike",
                new BikeEvent {
                    Id = bikeCreated.Id,
                    Name = bikeCreated.Name,
                    UserId = bikeCreated.UserId,
                    Latitude = bikeCreated.Latitude,
                    Longitude = bikeCreated.Longitude,
                    Status = exists == 0 ? BikeStatus.Created : BikeStatus.Updated,
                    EventDate = DateTime.UtcNow
                });

            return new BikeReply() {
                BikeId = bikeCreated.Id.ToString(),
                OperationResult = exists == 0 ? Operation.Create : Operation.Update
            };
        }

        private async Task<UserBikesResponse> ByUser (UserRequest request, ServerCallContext context)
        {
            var bikes = await bikeCollection.FindAsync(t => t.UserId == Guid.Parse(request.UserId));
            var response = new UserBikesResponse();
            foreach (var b in await bikes.ToListAsync()) {
                var userBike = new UserBikeResponse {
                    UserId = request.UserId,
                    BikeId = b.Id.ToString(),
                    Name = b.Name,
                    Latitude = b.Latitude,
                    Longitude = b.Longitude
                };
                response.Response.Add(userBike);
            }
            return response;
        }

        private async Task<BikeAggregatorsResponse> BikeAggregator (ServerCallContext context)
        {
            var bikeAggregator = await (await bikeAggregationCollection.FindAsync(FilterDefinition<BikeAggregation>.Empty)).ToListAsync();
            var response = new BikeAggregatorsResponse();

            foreach (var b in bikeAggregator) {
                response.Countries.Add(new BikeAggregatorResponse() { Country = b.Country, Count = b.Count });
                await daprClient.PublishEventAsync(
                   Environment.GetEnvironmentVariable("PUBSUB_NAME"),
                   "bike-aggregator-reader",
                   new BikeAggregatorEvent {
                       Country = b.Country,
                       EventDate = DateTime.UtcNow
                   });
            }
            return response;
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
                case "ByUser":
                    var inputByUser = request.Data.Unpack<UserRequest>();
                    var outputByUser = await ByUser(inputByUser, context);
                    response.Data = Any.Pack(outputByUser);
                    break;
                case "AggregatorBike":
                    var outputBikeAggregator = await BikeAggregator(context);
                    response.Data = Any.Pack(outputBikeAggregator);
                    break;
            }
            return response;
        }

        public override Task<ListTopicSubscriptionsResponse> ListTopicSubscriptions (Empty request, ServerCallContext context)
        {
            return Task.FromResult(new ListTopicSubscriptionsResponse());
        }

        public override Task<TopicEventResponse> OnTopicEvent (TopicEventRequest request, ServerCallContext context)
        {
            return Task.FromResult(new TopicEventResponse());
        }
    }
}
