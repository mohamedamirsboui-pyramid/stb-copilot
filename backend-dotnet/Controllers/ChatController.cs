using Microsoft.AspNetCore.Mvc;
using StbCopilot.Api.Models;
using StbCopilot.Api.Services;

namespace StbCopilot.Api.Controllers;

[ApiController]
[Route("api")]
public class ChatController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IDocumentService _documentService;
    private readonly IRetrieverService _retrieverService;
    private readonly IAnswerService _answerService;
    private readonly IAccessLogger _accessLogger;

    public ChatController(
        IAuthService authService,
        IDocumentService documentService,
        IRetrieverService retrieverService,
        IAnswerService answerService,
        IAccessLogger accessLogger)
    {
        _authService = authService;
        _documentService = documentService;
        _retrieverService = retrieverService;
        _answerService = answerService;
        _accessLogger = accessLogger;
    }

    [HttpPost("ask")]
    public IActionResult Ask([FromBody] AskRequest request)
    {
        try
        {
            // Check authentication
            var sessionToken = GetSessionToken();
            var user = _authService.VerifySession(sessionToken);

            if (user == null)
            {
                return Unauthorized(new { error = "Non autorisé. Veuillez vous connecter." });
            }

            var question = request.Question?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(question))
            {
                return BadRequest(new { error = "Question is required" });
            }

            Console.WriteLine($"\n📝 Question from {user.Email}: {question}");

            // Retrieve relevant chunks
            var chunksText = _documentService.GetChunksTextOnly();
            var relevantChunks = _retrieverService.SimpleSearch(question, chunksText);
            Console.WriteLine($"✓ Found {relevantChunks.Count} relevant chunks");

            // Generate answer
            var result = _answerService.GenerateAnswerWithConfidence(question, relevantChunks);
            Console.WriteLine($"✓ Generated answer (confidence: {result.Confidence}% - {result.ConfidenceLabel})");

            // Log the question
            _accessLogger.LogQuestion(
                user.Email,
                question,
                result.Confidence,
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            // Build source metadata
            var sources = relevantChunks.Select(chunkText =>
            {
                var metadata = _documentService.GetChunkMetadata(chunkText);
                return metadata != null
                    ? new SourceChunk
                    {
                        Text = chunkText,
                        Document = metadata.SourceDocument,
                        Type = metadata.DocumentType,
                        Modified = metadata.Modified
                    }
                    : new SourceChunk
                    {
                        Text = chunkText,
                        Document = "unknown",
                        Type = "procedure"
                    };
            }).ToList();

            return Ok(new AskResponse
            {
                Question = question,
                Answer = result.Answer,
                Confidence = result.Confidence,
                ConfidenceLabel = result.ConfidenceLabel,
                Sources = sources
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    private string GetSessionToken()
    {
        var auth = Request.Headers.Authorization.ToString();
        return auth.StartsWith("Bearer ") ? auth["Bearer ".Length..] : auth;
    }
}
