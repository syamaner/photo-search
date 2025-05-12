namespace PhotoSearch.API.Models;

public static class ConnectionStrings
{
        public static string? OllamaConnectionString { get; } = Environment.GetEnvironmentVariable("ConnectionStrings__Ollama");
        public static string? OpenAIKey { get; } = Environment.GetEnvironmentVariable("OpenAIKey");

        public static string? OpenAIConnectionString { get; } = Environment.GetEnvironmentVariable("ConnectionStrings__openaiConnection");

        public static string? QdrantConnectionString { get; } = Environment.GetEnvironmentVariable("ConnectionStrings__qdrant");
        
        public static string? ChatModel { get; } = Environment.GetEnvironmentVariable("ConnectionStrings__chat-model");
        public static string? EmbeddingModel { get; } = Environment.GetEnvironmentVariable("ConnectionStrings__embedding-model");
}