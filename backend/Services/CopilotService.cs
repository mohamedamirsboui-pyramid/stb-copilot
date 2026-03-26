// ╔══════════════════════════════════════════════════════════╗
// ║  CopilotService.cs — The receptionist of our system      ║
// ║                                                          ║
// ║  Analogy: Imagine a receptionist at a bank office.       ║
// ║  The receptionist doesn't know all the answers, but      ║
// ║  they know WHO to ask. They take your question and       ║
// ║  forward it to the right expert (RagService).            ║
// ║                                                          ║
// ║  This is a thin wrapper — it just calls RagService.      ║
// ║  Why have it? Because the Controller shouldn't talk      ║
// ║  directly to the RAG pipeline. Separation of concerns.   ║
// ╚══════════════════════════════════════════════════════════╝

using StbCopilot.Api.Models;

namespace StbCopilot.Api.Services
{
    /// <summary>
    /// CopilotService is the intermediary between the Controller and RagService.
    /// It receives questions from the controller and forwards them to the RAG pipeline.
    /// </summary>
    public class CopilotService
    {
        // Reference to the RAG service (the "expert" who actually finds answers).
        private readonly RagService _ragService;

        // Constructor: .NET automatically injects the RagService here.
        // This is called "Dependency Injection" — .NET creates RagService once
        // and passes it to anyone who needs it.
        public CopilotService(RagService ragService)
        {
            _ragService = ragService;
        }

        /// <summary>
        /// Takes a question from the agent and returns an answer.
        /// This method simply forwards the question to the RAG pipeline.
        /// </summary>
        public async Task<AnswerResponse> GetAnswer(string question)
        {
            // Forward the question to the RAG pipeline.
            // The RAG service will:
            // 1. Convert the question to a vector
            // 2. Find the best matching document chunk
            // 3. Send it to Groq AI for a structured answer
            // 4. Return the answer with the source document name
            return await _ragService.AskQuestion(question);
        }
    }
}
