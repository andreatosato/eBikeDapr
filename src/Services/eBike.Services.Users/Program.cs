using System;
using Dapr.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddDapr();
var app = builder.Build();
app.UseCloudEvents();
app.MapSubscribeHandler();

if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}

app.MapGet("/{id}", (int id, DaprClient dapr) => dapr.GetStateAsync<User>("user-state", $"user-{id}"));

app.MapPost("/v1/User", async (User newUser, DaprClient dapr) => 
{
    await GetUserCollection().FindOneAndReplaceAsync(t => t.Id == newUser.Id, newUser);
    await dapr.PublishEventAsync("pubsub", "user-insert", newUser);
});

app.MapPost("/v1/UserInsert", (User newUser, DaprClient dapr) => dapr.SaveStateAsync<User>("user-state", $"user-{newUser.Id}", newUser))
   .WithTopic("pubsub", "user-insert");

await app.RunAsync();

IMongoCollection<User> GetUserCollection ()
{
    var client = new MongoClient(Environment.GetEnvironmentVariable("ConnectionString"));
    var database = client.GetDatabase("Users");
    var userCollection = database.GetCollection<User>("UsersCollection");
    return userCollection;
}