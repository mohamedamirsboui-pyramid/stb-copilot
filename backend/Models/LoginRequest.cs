// ╔══════════════════════════════════════════════════════════╗
// ║  LoginRequest.cs — What the user sends to log in         ║
// ║                                                          ║
// ║  Analogy: Like showing your employee badge at the bank   ║
// ║  entrance — you give your email and password, and the    ║
// ║  guard checks if you're allowed in.                       ║
// ╚══════════════════════════════════════════════════════════╝

namespace StbCopilot.Api.Models
{
    /// <summary>
    /// This class represents the LOGIN form data.
    /// The frontend sends email + password, and the backend
    /// checks if they match a known user.
    /// 
    /// Example JSON from frontend:
    /// {
    ///     "email": "agent@stb.tn",
    ///     "password": "agent123"
    /// }
    /// </summary>
    public class LoginRequest
    {
        // The user's email address (e.g., "agent@stb.tn").
        public required string Email { get; set; }

        // The user's password.
        // In a real app, we would hash this. For MVP, we compare directly.
        public required string Password { get; set; }
    }
}
