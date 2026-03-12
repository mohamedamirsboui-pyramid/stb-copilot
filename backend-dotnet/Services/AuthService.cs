using System.Security.Cryptography;
using System.Text;
using StbCopilot.Api.Models;

namespace StbCopilot.Api.Services;

public interface IAuthService
{
    UserInfo? Authenticate(string email, string password);
    string CreateSession(UserInfo user);
    UserInfo? VerifySession(string sessionToken);
    void DestroySession(string sessionToken);
}

public class AuthService : IAuthService
{
    private class StoredUser
    {
        public string PasswordHash { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    private class Session
    {
        public UserInfo User { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    private readonly Dictionary<string, StoredUser> _users;
    private readonly Dictionary<string, Session> _sessions = new();

    public AuthService()
    {
        _users = new Dictionary<string, StoredUser>
        {
            ["agent@stb.tn"] = new StoredUser
            {
                PasswordHash = HashPassword("agent123"),
                Name = "Agent STB",
                Role = "agent"
            },
            ["admin@stb.tn"] = new StoredUser
            {
                PasswordHash = HashPassword("admin123"),
                Name = "Administrateur",
                Role = "admin"
            }
        };
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public UserInfo? Authenticate(string email, string password)
    {
        email = email.ToLower().Trim();

        if (!_users.TryGetValue(email, out var user))
            return null;

        var passwordHash = HashPassword(password);

        if (passwordHash != user.PasswordHash)
            return null;

        return new UserInfo
        {
            Email = email,
            Name = user.Name,
            Role = user.Role
        };
    }

    public string CreateSession(UserInfo user)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        _sessions[token] = new Session
        {
            User = user,
            CreatedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddHours(8)
        };

        return token;
    }

    public UserInfo? VerifySession(string sessionToken)
    {
        if (string.IsNullOrEmpty(sessionToken) || !_sessions.TryGetValue(sessionToken, out var session))
            return null;

        if (DateTime.Now > session.ExpiresAt)
        {
            _sessions.Remove(sessionToken);
            return null;
        }

        return session.User;
    }

    public void DestroySession(string sessionToken)
    {
        _sessions.Remove(sessionToken);
    }
}
