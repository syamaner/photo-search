namespace PhotoSearch.AppHost;


public static class Constants
{
    public static class ConnectionStringNames
    {
        public const string Qdrant = "qdrant";
        public const string Ollama = "ollama";
        public const string ChatModel = "chat-model";
        public const string EmbeddingModel = "embedding-model";
        public const string ApiService = "api-service";
        public const string Ui = "ui-application";
        public const string JupyterService = "juptyer-service";        
        public const string FaqVectorName = "page_content_vector";    
        public const string MetadataPayloadFielname = "metadata";
        public const string FaqPayloadFieldName = "page_content";
        
        
    }
}