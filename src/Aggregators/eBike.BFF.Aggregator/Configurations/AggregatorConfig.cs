using Microsoft.Extensions.DependencyInjection;
using System;

namespace eBike.BFF.Aggregator
{
    public record AggregatorConfig ()
    {
        public string APPID_BIKES { get; set; }
        public string APPID_USERS { get; set; }
    }

    public static class AggregatorConfigExtension
    {
        public static IServiceCollection AddConfiguration (this IServiceCollection services)
        {
            services.AddOptions<AggregatorConfig>()
                .Configure((settings) => {
                    settings.APPID_BIKES = Environment.GetEnvironmentVariable("APPID_BIKES");
                    settings.APPID_USERS = Environment.GetEnvironmentVariable("APPID_USERS");
                });
            return services;
        }
    }
}
