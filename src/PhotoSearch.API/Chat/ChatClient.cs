using System.Text;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using PhotoSearch.API.Models;

#pragma warning disable SKEXP0001

namespace PhotoSearch.API.Chat;

public class ChatClient(
    Kernel kernel,
    ILogger<ChatClient> logger) : IChatClient
{
    private const short TopSearchResults = 10;
 
    public async Task<string> AnswerQuestion(string question, bool useAdditionalContext,  string embeddingModel)
    {
        try
        {
            if (!useAdditionalContext) return await AnswerWithoutAdditionalContext(question);

            var context = await GetContextFromVectorStore(question, embeddingModel);
            return await AnswerWithAdditionalContext(context, question);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Error while answering the question. Additional context required? : {useAdditionalContext}",
                useAdditionalContext);
            return "An error occured while answering the question.";
        }
    }

    private async Task<string> AnswerWithoutAdditionalContext(string question)
    {
        try
        {
            var arguments = new KernelArguments
            {
                { "question", question }
            };

            var kernelFunction = kernel.CreateFunctionFromPrompt(PromptConstants.BasicPromptConfig);
            var result = await kernelFunction.InvokeAsync(kernel, arguments);
            return result.ToString();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while answering the question without additional context.");
            return "An error occured while answering the question.";
        }
    }

    private async Task<string> AnswerWithAdditionalContext(string context, string question)
    {
        
        var arguments = new KernelArguments
        {
            { "context", context },
            { "question", question }            
        };

        var kernelFunction = kernel.CreateFunctionFromPrompt(PromptConstants.RagPromptConfig);
        var result = await kernelFunction.InvokeAsync(kernel, arguments);
        return result.ToString();
    }

    /// <summary>
    /// Get context from the vector store based on the question.
    ///  This method uses the vector store to search for the most relevant context based on the question:
    ///      1. Retrieve the embeddings using the embedding model
    ///      2. Search the vector store for the most relevant context based on the embeddings.
    ///      3. Return the context as a string.
    /// </summary>
    /// <param name="question"></param>
    /// <returns>Vector Search Results.</returns>
    private async Task<string> GetContextFromVectorStore(string question, string embeddingModel)
    {
        var vectorStore = kernel.GetRequiredService<IVectorStore>(embeddingModel);
        var faqCollection = vectorStore.GetCollection<Guid, PhotoIndexRecord>(embeddingModel);
        var embeddingGenerator =
            kernel.GetRequiredService<ITextEmbeddingGenerationService>(embeddingModel);
        var questionVectors =
            await embeddingGenerator.GenerateEmbeddingsAsync([question]);

        var stbContext = new StringBuilder();

        var searchResults = faqCollection.SearchEmbeddingAsync(questionVectors[0], TopSearchResults);

        await foreach (var item in searchResults)
        {
            stbContext.AppendLine(item.Record.Content);
        }

        return stbContext.ToString();
    }
    
    public async IAsyncEnumerable<string> GetChunks(string question, string embeddingModel)
    {
        var vectorStore = kernel.GetRequiredService<IVectorStore>(embeddingModel);
        var faqCollection = vectorStore.GetCollection<Guid, PhotoIndexRecord>(embeddingModel);
        var embeddingGenerator =
            kernel.GetRequiredService<ITextEmbeddingGenerationService>(embeddingModel);
        var questionVectors =
            await embeddingGenerator.GenerateEmbeddingsAsync([question]);


        var searchResults = faqCollection.SearchEmbeddingAsync(questionVectors[0], TopSearchResults);
        
        await foreach (var item in searchResults)
        {
           yield return item.Record.Content;
        }
 
    }
}