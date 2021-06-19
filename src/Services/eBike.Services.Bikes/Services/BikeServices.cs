using Dapr.Client;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static eBike.Services.Bikes.BikeEndpoint;

namespace eBike.Services.Bikes.Services
{
    public class BikeServices : BikeEndpointBase
    {
        private readonly ILogger<BikeServices> _logger;
        private readonly DaprClient daprClient;

        public BikeServices (ILogger<BikeServices> logger, DaprClient daprClient)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            this.daprClient = daprClient ?? throw new System.ArgumentNullException(nameof(daprClient));
        }

        public override Task<BikeReply> CreateOrUpdate (BikeRequest request, ServerCallContext context)
        {
            return base.CreateOrUpdate(request, context);
        }
    }
}
