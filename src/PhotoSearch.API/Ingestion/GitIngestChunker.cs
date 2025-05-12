using LangChain.Splitters.Text;

namespace PhotoSearch.API.Ingestion;

public class GitIngestChunker : IChunker
{
    private readonly Dictionary<string, TextSplitter> _splitters;

    private readonly CharacterTextSplitter _characterSplitter = new("\n", 600, 50);


    public async IAsyncEnumerable<FileChunks> GetChunks(string gitIngestFilePath)
    {   
        var splitter = _characterSplitter;

        var fileChunks = new FileChunks(gitIngestFilePath, []);

        var chunks = splitter.SplitText(File.ReadAllText(gitIngestFilePath));
        if (chunks.Any(x => x.Length > 600))
        {
            foreach (var chunk in chunks)
            {
                if (chunk.Length > 600)
                {
                    var subChunks = _characterSplitter.SplitText(chunk);
                    fileChunks.Chunks.AddRange(subChunks);
                }
                else
                {
                    fileChunks.Chunks.Add(chunk);
                }
            }
        }
        else
        {
            foreach (var chunk in chunks)
            {
                fileChunks.Chunks.Add(chunk);
            }
        }

        yield return fileChunks;

    }
}