using System.Text.Encodings.Web;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using PhotoSearch.Common.Contracts;
using PhotoSearch.Data;
using PhotoSearch.ServiceDefaults;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddRabbitMQClient("messaging");
builder.AddMasstransit();
builder.AddNpgsqlDbContext<PhotoSearchContext>("postgresdb");
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
app.Run();