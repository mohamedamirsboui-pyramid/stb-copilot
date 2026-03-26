// ╔══════════════════════════════════════════════════════════╗
// ║  AuthController.cs — The login door of our API           ║
// ║                                                          ║
// ║  Analogy: This is the bank's entrance door.              ║
// ║  People come here, show their credentials (email +       ║
// ║  password), and if they're valid, they get a pass (JWT). ║
// ║                                                          ║
// ║  URL: POST /api/auth/login                               ║
// ╚══════════════════════════════════════════════════════════╝

using Microsoft.AspNetCore.Mvc;
using StbCopilot.Api.Models;
using StbCopilot.Api.Services;

namespace StbCopilot.Api.Controllers
{
    // [ApiController] tells .NET: "This class handles HTTP requests."
    // [Route("api/auth")] means all URLs in this controller start with /api/auth.
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        // Reference to the authentication service.
        private readonly AuthService _authService;

        // Constructor: .NET automatically gives us the AuthService.
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// POST /api/auth/login
        /// 
        /// Receives email + password, returns a JWT token if valid.
        /// 
        /// Example request body:
        /// { "email": "agent@stb.tn", "password": "agent123" }
        /// 
        /// Example success response:
        /// { "token": "eyJ...", "userName": "Ahmed Ben Ali", "role": "Agent" }
        /// 
        /// Example failure response (401 Unauthorized):
        /// { "message": "Email ou mot de passe incorrect" }
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Ask the AuthService to check the credentials.
            var result = _authService.Login(request.Email, request.Password);

            // If login failed (result is null), return 401 Unauthorized.
            if (result == null)
            {
                return Unauthorized(new { message = "Email ou mot de passe incorrect" });
            }

            // If login succeeded, return the JWT token + user info.
            return Ok(result);
        }
    }
}
