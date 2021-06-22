using eBike.Workers.Processing;
using eBike.Workers.Processing.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System;

namespace eBike.Worker.Cron.Processing
{
    public class Startup
    {
        public Startup (IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services)
        {
            services.AddHttpClient("AzureMaps");
            services.AddScoped((sp) => {
                var client = new MongoClient(Environment.GetEnvironmentVariable("ConnectionString"));
                return client.GetDatabase("Bikes").GetCollection<BikeEntity>("BikesCollection");
            });
            services.AddScoped((sp) => {
                var client = new MongoClient(Environment.GetEnvironmentVariable("ConnectionString"));
                return client.GetDatabase("Bikes").GetCollection<BikeAggregation>("BikesAggregateCollection");
            });

            services.AddScoped<AggregateBikesCalculator>();
            services.AddControllers().AddDapr();
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "eBike.Worker.Cron.Processing", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "eBike.Worker.Cron.Processing v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
