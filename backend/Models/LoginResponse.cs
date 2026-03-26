// ╔══════════════════════════════════════════════════════════╗
// ║  LoginResponse.cs — What the API sends back after login  ║
// ║                                                          ║
// ║  Analogy: After the guard checks your badge, they give    ║
// ║  you a visitor pass (the JWT token). This pass has your   ║
// ║  name and role written on it. You show this pass every    ║
// ║  time you enter a room (make an API request).             ║
// ╚══════════════════════════════════════════════════════════╝

namespace StbCopilot.Api.Models
{
    /// <summary>
    /// This class represents what the backend sends back after
    /// a successful login. It contains:
    /// - A JWT token (the "pass" that proves who you are)
    /// - The user's name and role (for the UI to display)
    /// 
    /// Example JSON sent to frontend:
    /// {
    ///     "token": "eyJhbGciOiJIUzI...",
    ///     "userName": "Mohamed",
    ///     "role": "Agent"
    /// }
    /// </summary>
    public class LoginResponse
    {
        // The JWT token. The frontend stores this and sends it
        // with every API request in the Authorization header.
        public string Token { get; set; } = "";

        // The user's display name (shown in the navbar).
        public string UserName { get; set; } = "";

        // The user's role: "Agent" or "Admin".
        // The frontend uses this to show/hide certain features.
        public string Role { get; set; } = "";
    }
}
