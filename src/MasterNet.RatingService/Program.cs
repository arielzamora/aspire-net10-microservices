using MasterNet.RatingService;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddCors();

builder.AddRedisClient(connectionName: "cache");

builder.Services.AddSingleton<RatingStore>();

var app = builder.Build();

app.UseCors(x => x.AllowAnyOrigin());


app.MapPost("/ratings", ([FromBody]RatingRequest request, RatingStore ratingStore) =>
{
    Activity.Current?.AddEvent(new ActivityEvent("Ocurio un error"));

    if (string.IsNullOrWhiteSpace(request.Id))
    {
        return Results.BadRequest("Ingrese un valor valido");
    }
    ratingStore.AddRating(request.Id, request.Rating);
    Activity.Current?.SetTag("CourseId", request.Id);
    Activity.Current?.SetTag("Rating",request. Rating);
    return Results.Ok();
});
app.MapGet("/ratings",async ([FromQuery]string id, RatingStore ratingStore) =>
{
    if (string.IsNullOrWhiteSpace(id))
    {
        return Results.BadRequest("Ingrese un valor valido");
    }
    var rating = await ratingStore.GetAverageRating(id);

    Activity.Current?.SetTag("CourseId", id);
    Activity.Current?.SetTag("Rating", rating);

    return Results.Ok(rating);
});

app.MapDefaultEndpoints();

app.Run();

public record RatingRequest(string Id, int Rating);