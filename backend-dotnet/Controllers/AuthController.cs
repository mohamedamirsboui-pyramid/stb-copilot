using Microsoft.AspNetCore.Mvc;
using StbCopilot.Api.Models;
using StbCopilot.Api.Services;

namespace StbCopilot.Api.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAccessLogger _accessLogger;

    public AuthController(IAuthService authService, IAccessLogger accessLogger)
    {
        _authService = authService;
        _accessLogger = accessLogger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        try
        {
            var email = request.Email?.Trim() ?? string.Empty;
            var password = request.Password ?? string.Empty;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "Email et mot de passe requis"
                });
            }

            var user = _authService.Authenticate(email, password);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (user != null)
            {
                var sessionToken = _authService.CreateSession(user);
                _accessLogger.LogLogin(email, success: true, ipAddress);

                Console.WriteLine($"✓ Login successful: {email} ({user.Role})");

                return Ok(new LoginResponse
                {
                    Success = true,
                    SessionToken = sessionToken,
                    User = user
                });
            }

            _accessLogger.LogLogin(email, success: false, ipAddress);
            Console.WriteLine($"✗ Login failed: {email}");

            return Unauthorized(new LoginResponse
            {
                Success = false,
                Message = "Email ou mot de passe incorrect"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Login error: {ex.Message}");
            return StatusCode(500, new LoginResponse
            {
                Success = false,
                Message = "Erreur serveur"
            });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        try
        {
            var sessionToken = GetSessionToken();

            if (!string.IsNullOrEmpty(sessionToken))
            {
                var user = _authService.VerifySession(sessionToken);
                if (user != null)
                {
                    _accessLogger.LogLogout(user.Email, HttpContext.Connection.RemoteIpAddress?.ToString());
                }
                _authService.DestroySession(sessionToken);
            }

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Logout error: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Erreur serveur" });
        }
    }

    private string GetSessionToken()
    {
        var auth = Request.Headers.Authorization.ToString();
        return auth.StartsWith("Bearer ") ? auth["Bearer ".Length..] : auth;
    }
}
