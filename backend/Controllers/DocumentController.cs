// ╔══════════════════════════════════════════════════════════╗
// ║  DocumentController.cs — The admin's file cabinet         ║
// ║                                                          ║
// ║  Analogy: This is the filing room at the bank.           ║
// ║  Admins can:                                              ║
// ║  - Upload new procedure documents (PDFs)                  ║
// ║  - See all uploaded documents                             ║
// ║  - Delete old documents                                   ║
// ║                                                          ║
// ║  Only users with the "Admin" role can access this.        ║
// ║  Regular agents cannot upload or delete documents.        ║
// ╚══════════════════════════════════════════════════════════╝

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StbCopilot.Api.Services;

namespace StbCopilot.Api.Controllers
{
    // [Authorize(Roles = "Admin")] means: only Admin users can access this.
    // An Agent trying to access this will get 403 Forbidden.
    [ApiController]
    [Route("api/documents")]
    [Authorize(Roles = "Admin")]
    public class DocumentController : ControllerBase
    {
        // Reference to the RAG service (which stores documents and chunks).
        private readonly RagService _ragService;

        // Constructor: .NET gives us the RagService automatically.
        public DocumentController(RagService ragService)
        {
            _ragService = ragService;
        }

        /// <summary>
        /// POST /api/documents/upload
        /// 
        /// Upload a new PDF document. The RAG pipeline will:
        /// 1. Extract text from the PDF
        /// 2. Split it into chunks
        /// 3. Create TF-IDF vectors for each chunk
        /// 4. Store everything in memory
        /// 
        /// The request must be a multipart/form-data with:
        /// - file: the PDF file
        /// - category: the document category (e.g., "Procédures")
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string category = "Général")
        {
            // Check if a file was actually uploaded.
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Veuillez sélectionner un fichier PDF" });
            }

            // Check if the file is a PDF.
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Seuls les fichiers PDF sont acceptés" });
            }

            // Get the current user's email from the JWT token.
            // The "??" means: if not found, use "admin" as default.
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "admin";

            // Open the file as a stream and process it through the RAG pipeline.
            using var stream = file.OpenReadStream();
            var document = await _ragService.ProcessDocument(
                stream,
                file.FileName,
                category,
                userEmail
            );

            // Return the created document info.
            return Ok(new
            {
                message = "Document téléchargé et traité avec succès",
                document = document
            });
        }

        /// <summary>
        /// GET /api/documents
        /// 
        /// Returns the list of all uploaded documents.
        /// Used by the admin panel to display the documents table.
        /// </summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            var documents = _ragService.GetAllDocuments();
            return Ok(documents);
        }

        /// <summary>
        /// DELETE /api/documents/{id}
        /// 
        /// Deletes a document and all its chunks from memory.
        /// The document is identified by its unique ID.
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var success = _ragService.DeleteDocument(id);

            if (!success)
            {
                return NotFound(new { message = "Document non trouvé" });
            }

            return Ok(new { message = "Document supprimé avec succès" });
        }
    }
}
