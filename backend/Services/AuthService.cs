// ╔══════════════════════════════════════════════════════════╗
// ║  AuthService.cs — The security guard of our system       ║
// ║                                                          ║
// ║  Analogy: Think of a bank's security guard at the door.  ║
// ║  1. You show your badge (email + password)               ║
// ║  2. The guard checks if you're on the list               ║
// ║  3. If yes, gives you a visitor pass (JWT token)          ║
// ║  4. Your pass has your name and access level on it        ║
// ║  5. You show this pass every time you enter a room        ║
// ║                                                          ║
// ║  JWT = JSON Web Token = a secure, encoded "pass"          ║
// ╚══════════════════════════════════════════════════════════╝

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using StbCopilot.Api.Models;

namespace StbCopilot.Api.Services
{
    /// <summary>
    /// AuthService handles user authentication.
    /// For this MVP, users are HARDCODED (no database needed).
    /// In a real app, you would check against a database.
    /// </summary>
    public class AuthService
    {
        // The secret key used to sign JWT tokens.
        // This comes from appsettings.json.
        private readonly string _jwtSecret;

        // ═══════════════════════════════════════════════════════════
        // HARDCODED USERS — For MVP only!
        // In a real application, these would come from a database.
        // We store them as a simple list of tuples:
        // (email, password, display name, role)
        // ═══════════════════════════════════════════════════════════
        private readonly List<(string Email, string Password, string Name, string Role)> _users = new()
        {
            ("agent@stb.tn",  "agent123",  "Mohamed Amir Sboui", "Agent"),
            ("admin@stb.tn",  "admin123",  "Mohamed Amir Sboui", "Admin")
        };

        // Constructor: reads the JWT secret from configuration.
        public AuthService(IConfiguration configuration)
        {
            // Read the JWT secret key from appsettings.json.
            // This key is used to SIGN the tokens (like a stamp on the pass).
            _jwtSecret = configuration["Jwt:Secret"] ?? "StbCopilotDefaultSecretKey2026!";
        }

        /// <summary>
        /// Attempts to log in a user with email and password.
        /// Returns a LoginResponse with a JWT token if successful, or null if failed.
        /// </summary>
        public LoginResponse? Login(string email, string password)
        {
            // STEP 1: Check if the email and password match a known user.
            // We use FirstOrDefault to find the first user that matches.
            // The "u => ..." is a lambda (shortcut function):
            // it means "for each user u, check if email AND password match".
            var user = _users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password
            );

            // If no match found, return null (login failed).
            if (user == default) return null;

            // STEP 2: Create a JWT token for this user.
            string token = GenerateJwtToken(user.Email, user.Name, user.Role);

            // STEP 3: Return the token + user info.
            return new LoginResponse
            {
                Token = token,
                UserName = user.Name,
                Role = user.Role
            };
        }

        /// <summary>
        /// Generates a JWT token containing the user's identity.
        /// 
        /// A JWT token has 3 parts (separated by dots):
        /// 1. HEADER: says "this is a JWT, signed with HMAC-SHA256"
        /// 2. PAYLOAD: the user's data (email, name, role, expiration)
        /// 3. SIGNATURE: proves the token wasn't tampered with
        /// 
        /// Example: eyJhbGciOi.eyJlbWFpbCI.SflKxwRJSM
        /// </summary>
        private string GenerateJwtToken(string email, string name, string role)
        {
            // Create the "claims" — pieces of information about the user.
            // Think of claims as lines written on the visitor pass:
            // - Email: who you are
            // - Name: your display name
            // - Role: what you can access (Agent or Admin)
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role)
            };

            // Create the signing key from our secret string.
            // The key must be at least 32 characters long for security.
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSecret)
            );

            // Create the credentials (algorithm + key).
            // HMAC-SHA256 is a standard, secure signing algorithm.
            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            // Build the token with all the information.
            var token = new JwtSecurityToken(
                issuer: "StbCopilot",           // Who created this token
                audience: "StbCopilotUsers",     // Who can use this token
                claims: claims,                  // User info
                expires: DateTime.Now.AddHours(8), // Token valid for 8 hours (one work day)
                signingCredentials: credentials    // The signature
            );

            // Convert the token object to a string and return it.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
