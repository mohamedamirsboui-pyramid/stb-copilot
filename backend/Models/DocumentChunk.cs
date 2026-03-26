// ╔══════════════════════════════════════════════════════════╗
// ║  DocumentChunk.cs — A piece of text + its vector         ║
// ║                                                          ║
// ║  Analogy: Imagine cutting a book into paragraphs.        ║
// ║  Each paragraph is a "chunk". We also calculate a        ║
// ║  numeric fingerprint (vector) for each chunk so we can   ║
// ║  quickly find which paragraph answers a question.         ║
// ║                                                          ║
// ║  This is the HEART of the RAG pipeline.                  ║
// ╚══════════════════════════════════════════════════════════╝

namespace StbCopilot.Api.Models
{
    /// <summary>
    /// This class represents ONE CHUNK of text extracted from a PDF.
    /// 
    /// When a PDF is uploaded:
    /// 1. We extract all the text
    /// 2. We split it into chunks (paragraphs)
    /// 3. Each chunk gets converted into a VECTOR (list of numbers)
    /// 4. We store all chunks in memory so we can search them later
    /// 
    /// When an agent asks a question:
    /// 1. We convert the question into a vector too
    /// 2. We compare the question vector with ALL chunk vectors
    /// 3. The most similar chunk = the answer's source
    /// </summary>
    public class DocumentChunk
    {
        // Unique identifier for this chunk.
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // Which document this chunk came from (links to Document.Id).
        public string DocumentId { get; set; } = "";

        // The name of the source PDF file.
        // We store this so we can show "Source: procedure_xyz.pdf" in the answer.
        public string DocumentName { get; set; } = "";

        // The actual text content of this chunk (a paragraph or section).
        public string Content { get; set; } = "";

        // The VECTOR representation of this chunk's text.
        // A vector is just a list of numbers (double[]) that represents
        // the MEANING of the text. Similar texts have similar vectors.
        // We use TF-IDF to calculate these vectors (no paid API needed).
        public double[] Vector { get; set; } = Array.Empty<double>();
    }
}
