using Dapr.Client;
using eBike.Services.Bikes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eBike.BFF.Aggregator.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class BikesController : ControllerBase
    {
        private readonly DaprClient daprClient;
        private readonly AggregatorConfig aggregatorConfig;

        public BikesController (DaprClient daprClient, IOptions<AggregatorConfig> aggregatorConfig)
        {
            this.daprClient = daprClient ?? throw new System.ArgumentNullException(nameof(daprClient));
            this.aggregatorConfig = aggregatorConfig?.Value ?? throw new System.ArgumentNullException(nameof(aggregatorConfig));
        }

        [HttpPost("new-user-bike")]
        public async Task<IActionResult> Post (NewUserBike userBike)
        {
            if (userBike == null || userBike.User == null || userBike.Bike == null)
                return BadRequest();

            var userServiceViewModel = new UserServiceViewModel(
                userBike.User.Id ?? Guid.NewGuid(),
                userBike.User.Name,
                userBike.User.Surname,
                new()
            );
            await daprClient.InvokeMethodAsync(aggregatorConfig.UserAppId, "/v1/User", userServiceViewModel);

            var request = new BikeRequest() {
                Name = userBike.Bike.Name,
                UserId = userServiceViewModel.Id.ToString(),
                Latitude = userBike.Bike.Latitude,
                Longitude = userBike.Bike.Longitude
            };

            var reply = await daprClient.InvokeMethodGrpcAsync<BikeRequest, BikeReply>(aggregatorConfig.UserAppId, "CreateOrUpdate", request);

            return reply.OperationResult == Operation.Create
                ? StatusCode(StatusCodes.Status201Created, new { UserId = userServiceViewModel.Id, reply.BikeId })
                : StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    public record NewUserBike (User User, Bike Bike);
    public record User (Guid? Id, string Name, string Surname);
    public record Bike (string Name, double Latitude, double Longitude);

    public record UserServiceViewModel (Guid Id, string Name, string Surname, List<UserGarage> Garage);
    public record UserGarage (int[] Bikes);
}
