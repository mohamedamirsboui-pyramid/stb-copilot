// ╔══════════════════════════════════════════════════════════╗
// ║  CopilotController.cs — The chat desk of our API         ║
// ║                                                          ║
// ║  Analogy: This is the customer service desk at the bank. ║
// ║  Agents come here with questions, and we find answers     ║
// ║  by searching through our procedure documents.            ║
// ║                                                          ║
// ║  URL: POST /api/copilot/ask                              ║
// ╚══════════════════════════════════════════════════════════╝

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StbCopilot.Api.Models;
using StbCopilot.Api.Services;

namespace StbCopilot.Api.Controllers
{
    // [Authorize] means: you MUST have a valid JWT token to use this controller.
    // Without a token, you get 401 Unauthorized.
    [ApiController]
    [Route("api/copilot")]
    [Authorize]
    public class CopilotController : ControllerBase
    {
        // Reference to the Copilot service (which talks to the RAG pipeline).
        private readonly CopilotService _copilotService;

        // Constructor: .NET gives us the CopilotService automatically.
        public CopilotController(CopilotService copilotService)
        {
            _copilotService = copilotService;
        }

        /// <summary>
        /// POST /api/copilot/ask
        /// 
        /// The agent asks a question. The RAG pipeline finds the answer
        /// in the uploaded documents and returns it.
        /// 
        /// Requires: JWT token in the Authorization header.
        /// 
        /// Example request body:
        /// { "question": "Comment ouvrir un compte bancaire?" }
        /// 
        /// Example response:
        /// {
        ///     "answer": "Pour ouvrir un compte, vous devez...",
        ///     "sourceDocument": "procedure_compte.pdf",
        ///     "confidence": 0.85
        /// }
        /// </summary>
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] QuestionRequest request)
        {
            // Check if the question is empty.
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new { message = "La question ne peut pas être vide" });
            }

            // Forward the question to the service → RAG pipeline → Groq AI.
            var answer = await _copilotService.GetAnswer(request.Question);

            // Return the answer as JSON.
            return Ok(answer);
        }
    }
}
