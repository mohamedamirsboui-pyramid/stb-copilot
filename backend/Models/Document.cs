// ╔══════════════════════════════════════════════════════════╗
// ║  Document.cs — Represents an uploaded PDF file           ║
// ║                                                          ║
// ║  Analogy: Imagine a library catalog card. Each card      ║
// ║  has the book's name, category, when it was added,       ║
// ║  and whether it's still available. This class is          ║
// ║  that catalog card — but for our bank's PDF documents.    ║
// ╚══════════════════════════════════════════════════════════╝

namespace StbCopilot.Api.Models
{
    /// <summary>
    /// This class represents a PDF document uploaded by an admin.
    /// We store metadata about each document (name, category, etc.)
    /// so the admin panel can display and manage them.
    /// 
    /// The actual TEXT extracted from the PDF is stored separately
    /// in DocumentChunk objects (see DocumentChunk.cs).
    /// </summary>
    public class Document
    {
        // Unique identifier for this document.
        // We use Guid.NewGuid() to generate a random unique ID.
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // The original file name (e.g., "procedure_ouverture_compte.pdf").
        public string FileName { get; set; } = "";

        // Category for organizing documents (e.g., "Procédures", "Réglementation").
        public string Category { get; set; } = "";

        // Version number (e.g., "1.0", "2.1").
        // Useful if the same procedure gets updated.
        public string Version { get; set; } = "1.0";

        // Status: "Active" or "Archivé" (archived).
        // Only active documents are searched during RAG.
        public string Status { get; set; } = "Active";

        // When the document was uploaded.
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Who uploaded it (the admin's email).
        public string UploadedBy { get; set; } = "";
    }
}
