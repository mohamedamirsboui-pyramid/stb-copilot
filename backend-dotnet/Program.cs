using StbCopilot.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
    });

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register services as singletons (in-memory state)
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IRetrieverService, RetrieverService>();
builder.Services.AddSingleton<IAnswerService, AnswerService>();
builder.Services.AddSingleton<IAccessLogger>(new AccessLogger("logs"));

// Register DocumentService with docs path
var docsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "docs");
var documentService = new DocumentService(docsPath);
builder.Services.AddSingleton<IDocumentService>(documentService);

var app = builder.Build();

// Load documents at startup
Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("Loading Documents...");
Console.WriteLine(new string('=', 60) + "\n");

documentService.LoadAllDocuments();
documentService.ChunkAllDocuments();

var stats = documentService.GetDocumentStats();

app.UseCors();
app.MapControllers();

Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("STB Copilot - .NET API Server");
Console.WriteLine(new string('=', 60));
Console.WriteLine($"\n🚀 Starting server...");
Console.WriteLine($"📄 Documents loaded: {stats.TotalDocuments}");
Console.WriteLine($"📦 Total chunks: {stats.TotalChunks}");
Console.WriteLine($"\n🌐 API: http://localhost:5000/api/ask");
Console.WriteLine("\n" + new string('=', 60) + "\n");

app.Run("http://0.0.0.0:5000");
