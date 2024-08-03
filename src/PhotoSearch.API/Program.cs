using FastEndpoints;
using FastEndpoints.Swagger;
using Npgsql;
using PhotoSearch.Data;
using PhotoSearch.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddRabbitMQClient("messaging");
builder.AddMasstransit();
 
builder.AddNpgsqlDbContext<PhotoSearchContext>("photo-db",
    (b)=>{
        
});

builder.Services.AddFastEndpoints()
    .SwaggerDocument();
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