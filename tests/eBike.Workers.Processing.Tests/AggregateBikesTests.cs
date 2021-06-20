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
            var aggregateBikes = new AggregateBikes(mockHttpClientFactory.Object);
            Environment.SetEnvironmentVariable("AZURE_MAPS_KEY", "o4EHaprfguQb4eHiS7GMPPjbKn6GmZQtIa6_h_l_CIw");
            var r = await aggregateBikes.GetAddressAsync(42.6d, 10.5d);
        }
    }
}
