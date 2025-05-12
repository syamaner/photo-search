namespace PhotoSearch.API.Ingestion;

public interface IChunker
{
    IAsyncEnumerable<FileChunks> GetChunks(string gitIngestFilePath);
}