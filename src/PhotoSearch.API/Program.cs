using FastEndpoints;
using FastEndpoints.Swagger;
using MongoDB.Driver;
using OllamaSharp;
using PhotoSearch.Data.Models;
using PhotoSearch.ServiceDefaults;
 
var builder = WebApplication.CreateBuilder(args);
 
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.AddServiceDefaults();
builder.Services.AddCors();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddMasstransit();
builder.AddRabbitMQClient("messaging");
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

builder.Services.AddSingleton<IOllamaApiClient>(sp =>
{
    var lamaConnectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("Ollama");
    var httpClient = new HttpClient()
    {
        Timeout = TimeSpan.FromMinutes(5),
        BaseAddress = new Uri(lamaConnectionString!)
    };
    var client =new OllamaApiClient(httpClient);

    return client;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(b => b.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseFastEndpoints(c => c.Serializer.Options.PropertyNamingPolicy = null)
    .UseSwaggerGen();
app.Run();