#pragma warning disable SKEXP0070
#pragma warning disable SKEXP0010
using System.Diagnostics;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using PhotoSearch.API.Chat;
using PhotoSearch.API.Ingestion;
using PhotoSearch.API.Models;

namespace PhotoSearch.API.Extensions;

public static class RagExtensions
{
    private static readonly List<string> OpenAiModels = ["chatgpt-4o-latest", "text-embedding-3-large"];

    private const long HttpTimeoutMinutes = 10;

    public static void AddSemanticKernelModels(this WebApplicationBuilder builder)
    {
        // now  populate an in memory instance of ModelConfiguration
        var configuration = builder.Configuration.GetSection("ModelConfiguration").Get<ModelConfiguration>();
        Debug.Assert(configuration != null, nameof(configuration) + " != null");

        builder.Services.AddSingleton<IChatClient, ChatClient>();
        var kernelBuilder = Kernel.CreateBuilder();

        AddVectorStore(builder, kernelBuilder, configuration);
        AddEmbeddingModel(kernelBuilder, configuration);
        AddChatModel(kernelBuilder, configuration);

        var kernel = kernelBuilder.Build();
        builder.Services.AddSingleton(kernel);
    }

    private static void AddEmbeddingModel(IKernelBuilder kernelBuilder, ModelConfiguration configuration)
    {
        var apiKey = ConnectionStrings.OpenAIKey;

        List<string> embeddingModels = [configuration.EmbeddingModel];

        foreach (var model in embeddingModels)
        {
            if (OpenAiModels.Contains(model, StringComparer.OrdinalIgnoreCase) && !string.IsNullOrEmpty(apiKey))
            {
                kernelBuilder.AddOpenAITextEmbeddingGeneration(modelId: model,
                    apiKey: apiKey,
                    serviceId: model);
            }
            else
            {
                kernelBuilder.AddOllamaTextEmbeddingGeneration(model,
                    GetOllamaHttpClient(), //TODO use ollama url
                    model);
            }
        }
    }

    private static void AddChatModel(IKernelBuilder kernelBuilder, ModelConfiguration configuration)
    {
        List<string> models = [configuration.ChatModel];
        foreach (var model in models)
        {
            var apiKey = ConnectionStrings.OpenAIKey;
            if (OpenAiModels.Contains(model, StringComparer.OrdinalIgnoreCase) && !string.IsNullOrEmpty(apiKey))
            {
                kernelBuilder.AddOpenAIChatCompletion(model,
                    apiKey: apiKey,
                    serviceId: model);
            }
            else
            {
                kernelBuilder.AddOllamaChatCompletion(model,
                    GetOllamaHttpClient(), // TODO:  Ollama URL
                    serviceId: model);
            }
        }
    }

    private static void AddVectorStore(WebApplicationBuilder builder, IKernelBuilder kernelBuilder,
        ModelConfiguration configuration)
    {
        var connectionString = ConnectionStrings.QdrantConnectionString;
        var endpoint = connectionString?.Split(";")[0].Replace("Endpoint=", "");
        var key = connectionString?.Split(";")[1].Replace("Key=", "");

        List<string> embeddingModels = [configuration.EmbeddingModel];
        var parts = endpoint!.Split(":");
        var url = parts[1].Replace("//", "");

        var port = int.Parse(parts[2]);
        foreach (var embeddingModel in embeddingModels)
        {
            var options = new QdrantVectorStoreOptions
            {
                HasNamedVectors = true,
                VectorStoreCollectionFactory = new QdrantCollectionFactory(embeddingModel)
            };

            builder.Services.AddQdrantVectorStore(options: options, host: url, port: port, apiKey: key,
                serviceId: embeddingModel);

            kernelBuilder.AddQdrantVectorStore(options: options, host: url, port: port, apiKey: key,
                serviceId: embeddingModel);
        }
    }

    private static HttpClient GetOllamaHttpClient()
    {
        var ollamaUrl = ConnectionStrings.OllamaConnectionString;
        Debug.Assert(ollamaUrl != null, nameof(ollamaUrl) + " != null");
        return new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(HttpTimeoutMinutes),
            BaseAddress = new Uri(ollamaUrl)
        };
    }
}