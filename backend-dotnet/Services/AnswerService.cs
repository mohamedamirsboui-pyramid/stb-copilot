namespace StbCopilot.Api.Services;

public interface IAnswerService
{
    AnswerResult GenerateAnswerWithConfidence(string question, List<string> relevantChunks);
}

public class AnswerResult
{
    public string Answer { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string ConfidenceLabel { get; set; } = string.Empty;
}

public class AnswerService : IAnswerService
{
    public AnswerResult GenerateAnswerWithConfidence(string question, List<string> relevantChunks)
    {
        var answer = GenerateAnswer(question, relevantChunks);
        var confidence = CalculateConfidence(question, relevantChunks);
        var label = GetConfidenceLabel(confidence);

        return new AnswerResult
        {
            Answer = answer,
            Confidence = confidence,
            ConfidenceLabel = label
        };
    }

    private static string GenerateAnswer(string question, List<string> relevantChunks)
    {
        if (relevantChunks.Count == 0)
            return "I couldn't find relevant information to answer your question.";

        var combinedText = string.Join("\n\n", relevantChunks);
        var steps = ExtractNumberedSteps(combinedText);

        return steps.Count > 0
            ? FormatAsNumberedList(steps)
            : FormatAsBullets(relevantChunks);
    }

    private static double CalculateConfidence(string question, List<string> chunks)
    {
        if (chunks.Count == 0) return 0.0;

        // Factor 1: Number of chunks
        var chunkScore = Math.Min(chunks.Count / 3.0, 1.0) * 40;

        // Factor 2: Keyword overlap
        var questionWords = question.ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        var totalOverlap = 0;
        foreach (var chunk in chunks)
        {
            var chunkWords = chunk.ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .ToHashSet();

            totalOverlap += questionWords.Intersect(chunkWords).Count();
        }

        var overlapScore = Math.Min((double)totalOverlap / questionWords.Count, 1.0) * 60;

        return Math.Round(chunkScore + overlapScore, 1);
    }

    private static string GetConfidenceLabel(double confidence)
    {
        if (confidence >= 70) return "high";
        if (confidence >= 40) return "medium";
        return "low";
    }

    private static List<string> ExtractNumberedSteps(string text)
    {
        var steps = new List<string>();
        var lines = text.Split('\n');

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (line.StartsWith("Step ") && line.Contains(':'))
            {
                var stepContent = line.Split(':', 2)[1].Trim();
                if (!string.IsNullOrEmpty(stepContent))
                    steps.Add(stepContent);
            }
            else if (line.StartsWith("- ") && line.Length > 2)
            {
                var stepContent = line[2..].Trim();
                if (!string.IsNullOrEmpty(stepContent))
                    steps.Add(stepContent);
            }
        }

        return steps;
    }

    private static string FormatAsNumberedList(List<string> steps)
    {
        return string.Join("\n", steps.Select((s, i) => $"{i + 1}. {s}"));
    }

    private static string FormatAsBullets(List<string> chunks)
    {
        return string.Join("\n", chunks.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => $"• {c.Trim()}"));
    }
}
