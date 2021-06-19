using Microsoft.Extensions.DependencyInjection;
using System;

namespace eBike.BFF.Aggregator
{
    public record AggregatorConfig ()
    {
        public string BikeAppId { get; set; }
        public string UserAppId { get; set; }
    }

    public static class AggregatorConfigExtension
    {
        public static IServiceCollection AddConfiguration (this IServiceCollection services)
        {
            services.AddOptions<AggregatorConfig>()
                .Configure((settings) => {
                    settings.BikeAppId = Environment.GetEnvironmentVariable("BikeAppId");
                    settings.UserAppId = Environment.GetEnvironmentVariable("UserAppId");
                });
            return services;
        }
    }
}
