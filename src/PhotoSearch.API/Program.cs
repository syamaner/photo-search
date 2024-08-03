using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PhotoSearch.Common.Contracts;
using PhotoSearch.Data;
using PhotoSearch.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddRabbitMQClient("messaging");
builder.AddMasstransit();

NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
builder.AddNpgsqlDbContext<PhotoSearchContext>("photo-db");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/photos/index/{directory}", async (string directory, IBus bus) =>
    {
        await bus.Publish(new ImportPhotos(Uri.UnescapeDataString(directory)));
        return "Message sent!";
    }).WithName("IndexPhotos")
    .WithOpenApi();


app.MapGet("/photos/summarise/{modelName}", async (string modelName, IBus bus, PhotoSearchContext photoSearchContext) =>
    {
        var pathsWithoutSummary = await photoSearchContext.Photos
            .Select(p => p.ExactPath).ToListAsync();
        if (!pathsWithoutSummary.Any()) return "no photos to summarise!";

        await bus.Publish(new SummarisePhotos(pathsWithoutSummary, modelName));
        return "Message sent!";
    }).WithName("SummarisePhotos")
    .WithOpenApi();

app.MapGet("/photos", async (PhotoSearchContext photoSearchContext) =>
    {
        var photos = await photoSearchContext.Photos.ToListAsync();
        var results = photos.OrderBy(x => new Random(Environment.TickCount).Next())
            .Take(5).Select(x => new
            {
                x.RelativePath, Summary = x.PhotoSummaries?
                    .Where(y => y.Key.Contains("flor", StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault().Value.Description
            }).ToList();

        return TypedResults.Ok(results);
    }).WithName("GetPhotos")
    .WithOpenApi();

app.Run();