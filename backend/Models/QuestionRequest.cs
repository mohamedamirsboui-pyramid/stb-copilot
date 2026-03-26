// ╔══════════════════════════════════════════════════════════╗
// ║  QuestionRequest.cs — What the agent sends to ask       ║
// ║                                                          ║
// ║  Think of this like a form you fill out at a bank:       ║
// ║  "What is your question?" → you write it down → hand it  ║
// ║  to the employee. This class IS that form.                ║
// ╚══════════════════════════════════════════════════════════╝

namespace StbCopilot.Api.Models
{
    /// <summary>
    /// This class represents the QUESTION that an agent sends.
    /// When the frontend sends a POST request, .NET automatically
    /// converts the JSON body into this C# object.
    /// 
    /// Example JSON from frontend:
    /// {
    ///     "question": "Comment ouvrir un compte bancaire?"
    /// }
    /// </summary>
    public class QuestionRequest
    {
        // The question text typed by the agent in the chat box.
        // "required" means the API will reject requests without this field.
        public required string Question { get; set; }
    }
}
