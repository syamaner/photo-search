using FastEndpoints;
using FastEndpoints.Swagger;
using MongoDB.Driver;
using PhotoSearch.Data.Models;
using PhotoSearch.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
 
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddRabbitMQClient("messaging");
builder.AddMasstransit();
 
builder.AddMongoDBClient("MongoConnection");

builder.Services.AddFastEndpoints()
    .SwaggerDocument();
builder.AddMongoDBClient("photo-search");

builder.Services.AddScoped<IMongoCollection<Photo>>(x =>
{
    var mongoClient = x.GetRequiredService<IMongoClient>();
    const string collectionName = "photos";
    var db = mongoClient.GetDatabase("photo-search"); 
    return db.GetCollection<Photo>(collectionName); 
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
 
app.UseFastEndpoints(c => c.Serializer.Options.PropertyNamingPolicy = null)
    .UseSwaggerGen();
app.Run();