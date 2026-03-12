using System.Text.Json;

namespace StbCopilot.Api.Services;

public interface IAccessLogger
{
    void LogLogin(string userEmail, bool success, string? ipAddress = null);
    void LogLogout(string userEmail, string? ipAddress = null);
    void LogQuestion(string userEmail, string question, double confidence, string? ipAddress = null);
}

public class AccessLogger : IAccessLogger
{
    private readonly string _logFile;

    public AccessLogger(string logDirectory = "logs")
    {
        Directory.CreateDirectory(logDirectory);
        _logFile = Path.Combine(logDirectory, "access.log");
    }

    private void Log(string action, string userEmail, Dictionary<string, object>? details = null, string? ipAddress = null)
    {
        var entry = new Dictionary<string, object>
        {
            ["timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            ["action"] = action,
            ["user"] = userEmail,
            ["ip_address"] = ipAddress ?? "unknown",
            ["details"] = details ?? new Dictionary<string, object>()
        };

        try
        {
            var json = JsonSerializer.Serialize(entry);
            File.AppendAllText(_logFile, json + "\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to log file: {ex.Message}");
        }
    }

    public void LogLogin(string userEmail, bool success, string? ipAddress = null)
    {
        Log("login", userEmail, new Dictionary<string, object> { ["success"] = success }, ipAddress);
    }

    public void LogLogout(string userEmail, string? ipAddress = null)
    {
        Log("logout", userEmail, ipAddress: ipAddress);
    }

    public void LogQuestion(string userEmail, string question, double confidence, string? ipAddress = null)
    {
        Log("ask_question", userEmail, new Dictionary<string, object>
        {
            ["question"] = question,
            ["confidence"] = confidence
        }, ipAddress);
    }
}
