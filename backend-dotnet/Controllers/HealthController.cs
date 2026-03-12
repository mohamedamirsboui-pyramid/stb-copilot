using Microsoft.AspNetCore.Mvc;
using StbCopilot.Api.Models;
using StbCopilot.Api.Services;

namespace StbCopilot.Api.Controllers;

[ApiController]
[Route("api")]
public class HealthController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public HealthController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        var stats = _documentService.GetDocumentStats();
        return Ok(new HealthResponse
        {
            Status = "healthy",
            DocumentsLoaded = stats.TotalDocuments,
            ChunksCount = stats.TotalChunks,
            Documents = stats.Documents
        });
    }
}
