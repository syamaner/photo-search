using MassTransit;
using PhotoSearch.Common.Contracts;
using PhotoSearch.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddRabbitMQClient("messaging");
builder.AddMasstransit();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/photos/index", async (string directory, IBus bus) =>
    {
        await bus.Publish(new ImportPhotos(directory));
        return "Message sent!";
    }).WithName("IndexPhotos")
    .WithOpenApi();

app.Run();