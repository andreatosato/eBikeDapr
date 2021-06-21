using Dapr.Client;
using eBike.Commons.Events;
using eBike.Services.Users;
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
    var db = client.GetDatabase("Users");
    return db.GetCollection<User>("UsersCollection");
});
var app = builder.Build();
app.UseCloudEvents();
app.MapSubscribeHandler();

if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}

app.MapGet("/v1/User/{id}", (string id, [FromServices] DaprClient dapr) => dapr.GetStateAsync<User>(USER_STATE_NAME, $"user-{id}"));

app.MapPost("/v1/User", async (User newUser, [FromServices] DaprClient dapr, [FromServices] IMongoCollection<User> userCollection) => {
    var options = new FindOneAndReplaceOptions<User>() {
        ReturnDocument = ReturnDocument.After,
        IsUpsert = true
    };
    var userInsert = await userCollection.FindOneAndReplaceAsync<User>(t => t.Id == newUser.Id, newUser, options);
    var @event = new UserEvent() { Id = userInsert.Id, Name = userInsert.Name, Surname = userInsert.Surname, EventDate = DateTime.UtcNow };
    await dapr.PublishEventAsync(PUBSUB_NAME, "user-insert", (dynamic)@event);
    return new OkResult();
});

app.MapPut("/v1/User", async (User newUser, [FromServices] DaprClient dapr, [FromServices] IMongoCollection<User> userCollection) => {
    var options = new FindOneAndReplaceOptions<User>() {
        ReturnDocument = ReturnDocument.After,
        IsUpsert = true
    };
    var userUpdate = await userCollection.FindOneAndReplaceAsync<User>(t => t.Id == newUser.Id, newUser, options);
    var @event = new UserEvent() { Id = userUpdate.Id, Name = userUpdate.Name, Surname = userUpdate.Surname, EventDate = DateTime.UtcNow };
    await dapr.PublishEventAsync(PUBSUB_NAME, "user-update", (dynamic)@event);
    return new OkResult();
});

app.MapPost("/v1/UserInsert", (User newUser, [FromServices] DaprClient dapr) => dapr.SaveStateAsync<User>(USER_STATE_NAME, $"user-{newUser.Id}", newUser))
   .WithTopic(PUBSUB_NAME, "user-insert")
   .WithTopic(PUBSUB_NAME, "user-update");

await app.RunAsync();