using Aspire.Hosting;
using PhotoSearch.Ollama;

var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder.AddOllama();

builder.Build().Run();