namespace StbCopilot.Api.Models;

public class LoginResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? SessionToken { get; set; }
    public UserInfo? User { get; set; }
}

public class UserInfo
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
