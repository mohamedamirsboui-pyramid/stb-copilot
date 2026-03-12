namespace StbCopilot.Api.Models;

public class HealthResponse
{
    public string Status { get; set; } = "healthy";
    public int DocumentsLoaded { get; set; }
    public int ChunksCount { get; set; }
    public List<DocumentInfo> Documents { get; set; } = new();
}

public class DocumentInfo
{
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Modified { get; set; } = string.Empty;
}
