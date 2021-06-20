using Dapr.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System;

var PUBSUB_NAME = Environment.GetEnvironmentVariable("PUBSUB_NAME");
var USER_STATE_NAME = Environment.GetEnvironmentVariable("USER_STATE_NAME");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddDapr();
builder.Services.AddScoped<IMongoCollection<User>>((sp) => {
    var client = new MongoClient(Environment.GetEnvironmentVariable("ConnectionString"));
    return client.GetDatabase("Users").GetCollection<User>("UsersCollection");
});
var app = builder.Build();
app.UseCloudEvents();
app.MapSubscribeHandler();

if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}

app.MapGet("/{id}", (int id, [FromServices] DaprClient dapr) => dapr.GetStateAsync<User>(USER_STATE_NAME, $"user-{id}"));

app.MapPost("/v1/User", async (User newUser, [FromServices] DaprClient dapr, [FromServices] IMongoCollection<User> userCollection) => {
    await userCollection.FindOneAndReplaceAsync(t => t.Id == newUser.Id, newUser);
    await dapr.PublishEventAsync(PUBSUB_NAME, "user-insert", newUser);
});

app.MapPost("/v1/UserInsert", (User newUser, [FromServices] DaprClient dapr) => dapr.SaveStateAsync<User>(USER_STATE_NAME, $"user-{newUser.Id}", newUser))
   .WithTopic(PUBSUB_NAME, "user-insert");

await app.RunAsync();