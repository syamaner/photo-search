namespace PhotoSearch.API.Chat;

public interface IChatClient
{
    Task<string> AnswerQuestion(string question, bool useAdditionalContext, string embeddingModel);
    IAsyncEnumerable<string> GetChunks(string question, string embeddingModel);
}