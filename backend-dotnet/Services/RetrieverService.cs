namespace StbCopilot.Api.Services;

public interface IRetrieverService
{
    List<string> SimpleSearch(string question, List<string> chunks);
}

public class RetrieverService : IRetrieverService
{
    /// <summary>
    /// Simple keyword-based search (port of Python simple_search).
    /// Returns chunks that contain any word from the question.
    /// </summary>
    public List<string> SimpleSearch(string question, List<string> chunks)
    {
        var questionWords = question.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var results = new List<string>();

        foreach (var chunk in chunks)
        {
            var chunkLower = chunk.ToLower();
            if (questionWords.Any(word => chunkLower.Contains(word)))
            {
                results.Add(chunk);
            }
        }

        return results;
    }
}
