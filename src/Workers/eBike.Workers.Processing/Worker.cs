using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace eBike.Workers.Processing
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker (ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync (CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested) {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                try {
                    await Task.Delay(System.TimeSpan.FromMinutes(10).Milliseconds, stoppingToken);
                }
                catch (OperationCanceledException) {
                    return;
                }
            }
        }
    }
}
