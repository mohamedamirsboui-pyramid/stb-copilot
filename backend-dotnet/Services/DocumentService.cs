using StbCopilot.Api.Models;

namespace StbCopilot.Api.Services;

public interface IDocumentService
{
    void LoadAllDocuments();
    List<ChunkWithMetadata> ChunkAllDocuments();
    List<string> GetChunksTextOnly();
    ChunkMetadata? GetChunkMetadata(string chunkText);
    DocumentStats GetDocumentStats();
}

public class ChunkWithMetadata
{
    public string Text { get; set; } = string.Empty;
    public string SourceDocument { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Modified { get; set; } = string.Empty;
}

public class ChunkMetadata
{
    public string SourceDocument { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Modified { get; set; } = string.Empty;
}

public class DocumentStats
{
    public int TotalDocuments { get; set; }
    public int TotalChunks { get; set; }
    public List<DocumentInfo> Documents { get; set; } = new();
}

public class DocumentRecord
{
    public string Filename { get; set; } = string.Empty;
    public string Filepath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Modified { get; set; } = string.Empty;
    public string Type { get; set; } = "procedure";
}

public class DocumentService : IDocumentService
{
    private readonly string _docsDirectory;
    private readonly List<DocumentRecord> _documents = new();
    private readonly List<ChunkWithMetadata> _chunksWithMetadata = new();

    public DocumentService(string docsDirectory)
    {
        _docsDirectory = docsDirectory;
    }

    public void LoadAllDocuments()
    {
        _documents.Clear();

        if (!Directory.Exists(_docsDirectory))
        {
            Console.WriteLine($"✗ Documents directory not found: {_docsDirectory}");
            return;
        }

        foreach (var filepath in Directory.GetFiles(_docsDirectory, "*.txt"))
        {
            try
            {
                var filename = Path.GetFileName(filepath);
                var content = File.ReadAllText(filepath);
                var fileInfo = new FileInfo(filepath);

                _documents.Add(new DocumentRecord
                {
                    Filename = filename,
                    Filepath = filepath,
                    Content = content,
                    Size = fileInfo.Length,
                    Modified = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Type = "procedure"
                });

                Console.WriteLine($"✓ Loaded: {filename}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error loading {filepath}: {ex.Message}");
            }
        }

        Console.WriteLine($"\n✓ Total documents loaded: {_documents.Count}");
    }

    public List<ChunkWithMetadata> ChunkAllDocuments()
    {
        _chunksWithMetadata.Clear();

        foreach (var doc in _documents)
        {
            var chunks = ChunkText(doc.Content);

            foreach (var chunk in chunks)
            {
                _chunksWithMetadata.Add(new ChunkWithMetadata
                {
                    Text = chunk,
                    SourceDocument = doc.Filename,
                    DocumentType = doc.Type,
                    Modified = doc.Modified
                });
            }
        }

        Console.WriteLine($"✓ Total chunks created: {_chunksWithMetadata.Count}");
        return _chunksWithMetadata;
    }

    public List<string> GetChunksTextOnly()
    {
        return _chunksWithMetadata.Select(c => c.Text).ToList();
    }

    public ChunkMetadata? GetChunkMetadata(string chunkText)
    {
        var chunk = _chunksWithMetadata.FirstOrDefault(c => c.Text == chunkText);
        if (chunk == null) return null;

        return new ChunkMetadata
        {
            SourceDocument = chunk.SourceDocument,
            DocumentType = chunk.DocumentType,
            Modified = chunk.Modified
        };
    }

    public DocumentStats GetDocumentStats()
    {
        return new DocumentStats
        {
            TotalDocuments = _documents.Count,
            TotalChunks = _chunksWithMetadata.Count,
            Documents = _documents.Select(d => new DocumentInfo
            {
                Name = d.Filename,
                Size = d.Size,
                Modified = d.Modified
            }).ToList()
        };
    }

    /// <summary>
    /// Split text into chunks by double newlines (port of Python chunk_text).
    /// </summary>
    private static List<string> ChunkText(string text)
    {
        return text.Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
            .Where(c => !string.IsNullOrEmpty(c))
            .ToList();
    }
}
