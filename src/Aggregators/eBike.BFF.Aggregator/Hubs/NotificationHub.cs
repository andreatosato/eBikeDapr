using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace eBike.BFF.Aggregator.Hubs
{
    public class NotificationHub : Hub
    {
        public override Task OnConnectedAsync ()
        {
            return base.OnConnectedAsync();
        }
    }
}
