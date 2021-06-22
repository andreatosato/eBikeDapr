using Dapr;
using eBike.BFF.Aggregator.Hubs;
using eBike.Commons.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace eBike.BFF.Aggregator.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> hubContext;

        public NotificationController (IHubContext<NotificationHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        //.WithTopic (PUBSUB_NAME, "user-insert")
        //.WithTopic (PUBSUB_NAME, "user-update")

        [HttpPost("user-insert")]
        [Topic("bike-pubsub", "user-insert")]
        public async Task<IActionResult> PostUserInsert (UserEvent newUser)
        {
            await hubContext.Clients.All.SendAsync("user-insert", newUser);
            return Ok();
        }

        // https://github.com/dapr/dotnet-sdk/commit/a65e8b287d4c48f93952221cb1c6a1b1346ece26
        [HttpPost("user-update")]
        [Topic("bike-pubsub", "user-update")]
        public async Task<IActionResult> PostUserUpdate (UserEvent newUser)
        {
            await hubContext.Clients.All.SendAsync("user-update", newUser);
            return Ok();
        }

        [HttpPost("bike")]
        [Topic("bike-pubsub", "bike")]
        public async Task<IActionResult> PostUserUpdate (BikeEvent bikeEvent)
        {
            await hubContext.Clients.All.SendAsync("bike", bikeEvent);
            return Ok();
        }
    }
}
