using System;
using System.Text.Json;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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

app.MapGet("/{id}", async (http) => {
    if (!http.Request.RouteValues.TryGetValue("id", out var id)) {
        http.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    var dapr = http.RequestServices.GetRequiredService<DaprClient>();
    await http.Response.WriteAsJsonAsync<User>(
        await dapr.GetStateAsync<User>("user-state", $"user-{id}")
    );
});

app.MapPost("/v1/User", async (http) => 
{
    var newUser = await JsonSerializer.DeserializeAsync<User>(http.Request.Body);
    var dapr = http.RequestServices.GetRequiredService<DaprClient>();

    await GetUserCollection().FindOneAndReplaceAsync(t => t.Id == newUser.Id, newUser);

    await dapr.PublishEventAsync("pubsub", "user-insert", newUser);
});

app.MapPost("/v1/UserInsert", async (http) => {
    var newUser = await JsonSerializer.DeserializeAsync<User>(http.Request.Body);
    var dapr = http.RequestServices.GetRequiredService<DaprClient>();
    await dapr.SaveStateAsync<User>("user-state", $"user-{newUser.Id}", newUser);
}).WithTopic("pubsub", "user-insert");

await app.RunAsync();

IMongoCollection<User> GetUserCollection ()
{
    var client = new MongoClient(Environment.GetEnvironmentVariable("ConnectionString"));
    var database = client.GetDatabase("Users");
    var userCollection = database.GetCollection<User>("UsersCollection");
    return userCollection;
}