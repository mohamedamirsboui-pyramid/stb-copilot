// ╔════════════════════════════════════════════════════════════════════╗
// ║  RagService.cs — THE BRAIN OF STB COPILOT                        ║
// ║                                                                    ║
// ║  RAG = Retrieval-Augmented Generation                              ║
// ║                                                                    ║
// ║  Analogy: Imagine a LIBRARIAN at STB Bank.                        ║
// ║  1. The admin gives the librarian new procedure manuals (PDFs)     ║
// ║  2. The librarian reads each manual and takes notes (chunks)       ║
// ║  3. The librarian organizes notes by topic (vectors)               ║
// ║  4. When an agent asks a question, the librarian:                  ║
// ║     a. Understands the question                                    ║
// ║     b. Finds the most relevant note                                ║
// ║     c. Writes a clear answer based on that note                    ║
// ║     d. Tells the agent which manual the answer came from           ║
// ║                                                                    ║
// ║  This file implements ALL 10 steps of the RAG pipeline.            ║
// ╚════════════════════════════════════════════════════════════════════╝

using System.Text;
using System.Text.Json;
using StbCopilot.Api.Models;
using UglyToad.PdfPig;               // Library to read PDF files
using UglyToad.PdfPig.Content;        // To access individual pages

namespace StbCopilot.Api.Services
{
    /// <summary>
    /// RagService handles the entire RAG (Retrieval-Augmented Generation) pipeline.
    /// This version is refactored for thread-safety and robust error handling.
    /// </summary>
    public class RagService
    {
        // ═══════════════════════════════════════════════════════════
        // IN-MEMORY STORAGE
        // ═══════════════════════════════════════════════════════════
        private readonly List<Document> _documents = new();
        private readonly List<DocumentChunk> _chunks = new();
        private readonly Dictionary<string, int> _documentFrequency = new();
        private int _totalChunks = 0;

        // HttpClient to call the Groq API.
        // We use a single instance but create new HttpRequestMessages for thread safety.
        private readonly HttpClient _httpClient;
        private readonly string _groqApiKey;

        public RagService(IConfiguration configuration)
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Read API key and ensure it's trimmed of any accidental spaces.
            _groqApiKey = (configuration["Groq:ApiKey"] ?? "").Trim();
        }

        public List<Document> GetAllDocuments()
        {
            lock (_documents)
            {
                return _documents.ToList();
            }
        }

        public bool DeleteDocument(string documentId)
        {
            lock (_documents)
            {
                var document = _documents.FirstOrDefault(d => d.Id == documentId);
                if (document == null) return false;

                _chunks.RemoveAll(c => c.DocumentId == documentId);
                _documents.Remove(document);
                RebuildVocabulary();
                RecalculateAllVectors();
                return true;
            }
        }

        public Task<Document> ProcessDocument(Stream pdfStream, string fileName, string category, string uploadedBy)
        {
            var document = new Document
            {
                FileName = fileName,
                Category = category,
                UploadedBy = uploadedBy
            };

            string fullText = ExtractTextFromPdf(pdfStream);
            List<string> textChunks = SplitTextIntoChunks(fullText);

            lock (_documents)
            {
                foreach (var chunkText in textChunks)
                {
                    _chunks.Add(new DocumentChunk
                    {
                        DocumentId = document.Id,
                        DocumentName = fileName,
                        Content = chunkText
                    });
                }

                RebuildVocabulary();
                RecalculateAllVectors();
                _documents.Add(document);
            }

            return Task.FromResult(document);
        }

        public async Task<AnswerResponse> AskQuestion(string question)
        {
            Console.WriteLine($"[RAG] [{DateTime.Now:HH:mm:ss}] Received question: \"{question}\"");

            if (_chunks.Count == 0)
            {
                Console.WriteLine("[RAG] No documents loaded. Returning default message.");
                return new AnswerResponse
                {
                    Answer = "Aucun document n'a été chargé. Veuillez demander à un administrateur de télécharger des documents de procédure.",
                    SourceDocument = "Aucun",
                    Confidence = 0
                };
            }

            double[] questionVector = CalculateTfIdfVector(question);
            
            DocumentChunk? bestChunk = null;
            double bestScore = -1;

            lock (_documents)
            {
                foreach (var chunk in _chunks)
                {
                    double similarity = CosineSimilarity(questionVector, chunk.Vector);
                    if (similarity > bestScore)
                    {
                        bestScore = similarity;
                        bestChunk = chunk;
                    }
                }
            }

            if (bestChunk == null || bestScore < 0.05)
            {
                Console.WriteLine($"[RAG] No relevant chunk found (Best score: {bestScore}).");
                return new AnswerResponse
                {
                    Answer = "Je n'ai pas trouvé d'information pertinente dans les documents disponibles. Veuillez reformuler votre question.",
                    SourceDocument = "Aucun",
                    Confidence = (double)Math.Round((decimal)bestScore, 2)
                };
            }

            Console.WriteLine($"[RAG] Found best chunk in \"{bestChunk.DocumentName}\" (Score: {bestScore}). Calling Groq...");
            string answer = await GenerateAnswerWithGroq(question, bestChunk.Content);

            return new AnswerResponse
            {
                Answer = answer,
                SourceDocument = bestChunk.DocumentName,
                Confidence = (double)Math.Round((decimal)bestScore, 2)
            };
        }

        private string ExtractTextFromPdf(Stream pdfStream)
        {
            var textBuilder = new StringBuilder();
            try {
                using (var pdfDocument = PdfDocument.Open(pdfStream))
                {
                    foreach (var page in pdfDocument.GetPages())
                    {
                        textBuilder.AppendLine(page.Text);
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"[RAG] PDF Extraction Error: {ex.Message}");
            }
            return textBuilder.ToString();
        }

        private List<string> SplitTextIntoChunks(string text)
        {
            var chunks = new List<string>();
            var paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var paragraph in paragraphs)
            {
                var cleaned = paragraph.Trim();
                if (cleaned.Length >= 30) chunks.Add(cleaned);
            }

            if (chunks.Count == 0 && text.Length > 30)
            {
                for (int i = 0; i < text.Length; i += 500)
                {
                    int length = Math.Min(500, text.Length - i);
                    var chunk = text.Substring(i, length).Trim();
                    if (chunk.Length >= 30) chunks.Add(chunk);
                }
            }
            return chunks;
        }

        private void RebuildVocabulary()
        {
            _documentFrequency.Clear();
            _totalChunks = _chunks.Count;

            foreach (var chunk in _chunks)
            {
                var uniqueWords = Tokenize(chunk.Content).Distinct();
                foreach (var word in uniqueWords)
                {
                    if (_documentFrequency.ContainsKey(word)) _documentFrequency[word]++;
                    else _documentFrequency[word] = 1;
                }
            }
        }

        private double[] CalculateTfIdfVector(string text)
        {
            var words = Tokenize(text);
            if (words.Length == 0) return Array.Empty<double>();

            var wordCounts = new Dictionary<string, int>();
            foreach (var word in words)
            {
                if (wordCounts.ContainsKey(word)) wordCounts[word]++;
                else wordCounts[word] = 1;
            }

            var vocabulary = _documentFrequency.Keys.ToList();
            var vector = new double[vocabulary.Count];

            for (int i = 0; i < vocabulary.Count; i++)
            {
                string vocabWord = vocabulary[i];
                double tf = wordCounts.ContainsKey(vocabWord) ? (double)wordCounts[vocabWord] / words.Length : 0;
                double idf = _documentFrequency.ContainsKey(vocabWord) && _totalChunks > 0 
                    ? Math.Log((double)_totalChunks / _documentFrequency[vocabWord]) : 0;
                vector[i] = tf * idf;
            }

            return vector;
        }

        private void RecalculateAllVectors()
        {
            foreach (var chunk in _chunks)
            {
                chunk.Vector = CalculateTfIdfVector(chunk.Content);
            }
        }

        private double CosineSimilarity(double[] vectorA, double[] vectorB)
        {
            if (vectorA.Length == 0 || vectorB.Length == 0) return 0;

            int maxLength = Math.Max(vectorA.Length, vectorB.Length);
            double dotProduct = 0;
            double magnitudeA = 0;
            double magnitudeB = 0;

            for (int i = 0; i < maxLength; i++)
            {
                double a = i < vectorA.Length ? vectorA[i] : 0;
                double b = i < vectorB.Length ? vectorB[i] : 0;
                dotProduct += a * b;
                magnitudeA += a * a;
                magnitudeB += b * b;
            }

            double denominator = Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB);
            return denominator == 0 ? 0 : dotProduct / denominator;
        }

        private string[] Tokenize(string text)
        {
            return text.ToLowerInvariant()
                .Split(new[] { ' ', '\n', '\r', '\t', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}', '"', '\'', '-', '/', '\\', '«', '»' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(word => word.Length > 2)
                .ToArray();
        }

        private async Task<string> GenerateAnswerWithGroq(string question, string context)
        {
            if (string.IsNullOrEmpty(_groqApiKey))
            {
                return $"[Mode hors-ligne]\n\nExtrait:\n\n{context}";
            }

            try
            {
                var requestBody = new
                {
                    model = "llama3-8b-8192",
                    messages = new[]
                    {
                        new { role = "system", content = "Tu es un assistant de la banque STB. Réponds en français de manière professionnelle en te basant UNIQUEMENT sur le contexte." },
                        new { role = "user", content = $"Contexte:\n{context}\n\nQuestion: {question}" }
                    },
                    temperature = 0.3,
                    max_tokens = 1024
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                
                // Safe Request Message (Better than setting DefaultHeaders on shared client)
                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
                request.Headers.Add("Authorization", $"Bearer {_groqApiKey}");
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                Console.WriteLine($"[RAG] [{DateTime.Now:HH:mm:ss}] START: Sending to Groq...");
                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[RAG] [{DateTime.Now:HH:mm:ss}] END: Response status {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[RAG] GROQ ERROR: {responseText}");
                    return $"Désolé, une erreur s'est produite lors de la communication avec l'IA ({response.StatusCode}).";
                }

                using var jsonDoc = JsonDocument.Parse(responseText);
                var answer = jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                
                return answer ?? "Je n'ai pas pu générer une réponse propre.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RAG] [{DateTime.Now:HH:mm:ss}] FATAL ERROR: {ex.Message}");
                return $"Erreur de connexion : {ex.Message}";
            }
        }
    }
}
