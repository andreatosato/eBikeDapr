using eBike.Workers.Processing.Entities;
using MongoDB.Driver;
using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace eBike.Workers.Processing.Tests
{
    public class AggregateBikesTests
    {
        [Fact]
        public async Task GetCountry ()
        {
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(t => t.CreateClient("AzureMaps")).Returns(new HttpClient());
            Environment.SetEnvironmentVariable("ConnectionString", "mongodb://dapr:daprPassword@mongodb:27017");
            var client = new MongoClient(Environment.GetEnvironmentVariable("ConnectionString"));
            var bikeEntityCollection = client.GetDatabase("Bikes").GetCollection<BikeEntity>("BikesCollection");
            var bikeEntityAggregatorCollection = client.GetDatabase("Bikes").GetCollection<BikeAggregation>("BikesAggregateCollection");

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
                    t.Id == bItem.Id,
                    bItem,
                    new FindOneAndReplaceOptions<BikeEntity, BikeEntity>() { IsUpsert = true });
            }

            var aggregateBikes = new AggregateBikesCalculator(mockHttpClientFactory.Object, bikeEntityCollection, bikeEntityAggregatorCollection);
            Environment.SetEnvironmentVariable("AZURE_MAPS_KEY", "o4EHaprfguQb4eHiS7GMPPjbKn6GmZQtIa6_h_l_CIw");
            await aggregateBikes.ExecuteAsync();
        }

        [Fact]
        public async Task GetAddress ()
        {
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(t => t.CreateClient("AzureMaps")).Returns(new HttpClient());
            Environment.SetEnvironmentVariable("ConnectionString", "mongodb://dapr:daprPassword@mongodb:27017");
            var client = new MongoClient(Environment.GetEnvironmentVariable("ConnectionString"));
            var bileEntityCollection = client.GetDatabase("Bikes").GetCollection<BikeEntity>("BikesCollection");
            var bikeEntityAggregatorCollection = client.GetDatabase("Bikes").GetCollection<BikeAggregation>("BikesAggregateCollection");

            var aggregateBikes = new AggregateBikesCalculator(mockHttpClientFactory.Object, bileEntityCollection, bikeEntityAggregatorCollection);
            Environment.SetEnvironmentVariable("AZURE_MAPS_KEY", "o4EHaprfguQb4eHiS7GMPPjbKn6GmZQtIa6_h_l_CIw");
            var r = await aggregateBikes.GetAddressAsync(45.3946377d, 10.9195844d);
        }
    }
}
