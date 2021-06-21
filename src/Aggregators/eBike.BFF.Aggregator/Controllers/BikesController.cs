using Dapr.Client;
using eBike.Services.Bikes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
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
                null
            );
            await daprClient.InvokeMethodAsync(aggregatorConfig.APPID_USERS, "/v1/User", userServiceViewModel);

            var request = new BikeRequest() {
                Name = userBike.Bike.Name,
                UserId = userServiceViewModel.Id.ToString(),
                Latitude = userBike.Bike.Latitude,
                Longitude = userBike.Bike.Longitude
            };

            var reply = await daprClient.InvokeMethodGrpcAsync<BikeRequest, BikeReply>(aggregatorConfig.APPID_BIKES, "CreateOrUpdate", request);

            var userBikeServiceViewModel = userServiceViewModel with { Garage = new UserGarage(new string[] { reply.BikeId }) };
            await daprClient.InvokeMethodAsync(HttpMethod.Put, aggregatorConfig.APPID_USERS, "/v1/User", userServiceViewModel);

            return reply.OperationResult == Operation.Create
                ? StatusCode(StatusCodes.Status201Created, new { UserId = userServiceViewModel.Id, reply.BikeId })
                : StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetBikeByUserAsync (string userId)
        {
            var user = await daprClient.InvokeMethodAsync<UserServiceViewModel>(HttpMethod.Get, aggregatorConfig.APPID_USERS, $"/v1/User/{userId}");
            var reply = await daprClient.InvokeMethodGrpcAsync<UserRequest, UserBikesResponse>(aggregatorConfig.APPID_BIKES, "ByUser", new UserRequest { UserId = user.Id.ToString() });
            var bikes = new List<Bike>();
            foreach (var r in reply.Response) {
                bikes.Add(new Bike(r.Name, r.Latitude, r.Longitude));
            }

            return Ok(new UserBikeServiceViewModel(user.Id, user.Name, user.Surname, bikes));
        }

        [HttpGet("aggregator")]
        public async Task<IActionResult> GetAggregatorAsync ()
        {
            var reply = await daprClient.InvokeMethodGrpcAsync<BikeAggregatorsResponse>(aggregatorConfig.APPID_BIKES, "AggregatorBike");
            var aggregator = new List<BikeAggregatorViewModel>();
            foreach (var r in reply.Countries) {
                aggregator.Add(new BikeAggregatorViewModel(r.Country, r.Count));
            }

            return Ok(aggregator);

        }
    }

    public record NewUserBike (User User, Bike Bike);
    public record User (Guid? Id, string Name, string Surname);
    public record Bike (string Name, double Latitude, double Longitude);

    public record UserServiceViewModel (Guid Id, string Name, string Surname, UserGarage Garage);
    public record UserGarage (string[] Bikes);
    public record UserBikeServiceViewModel (Guid Id, string Name, string Surname, List<Bike> Garage);

    public record BikeAggregatorViewModel (string Country, int Count);
}
