namespace StbCopilot.Api.Models;

public class AskResponse
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string ConfidenceLabel { get; set; } = string.Empty;
    public List<SourceChunk> Sources { get; set; } = new();
}

public class SourceChunk
{
    public string Text { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Modified { get; set; }
}
