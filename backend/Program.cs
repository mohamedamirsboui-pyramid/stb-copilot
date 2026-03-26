// ╔══════════════════════════════════════════════════════════╗
// ║  Program.cs — The main entrance of our backend            ║
// ║                                                          ║
// ║  Analogy: This is like opening a restaurant.             ║
// ║  Before customers arrive, you need to:                    ║
// ║  1. Hire employees (register services)                    ║
// ║  2. Set up security (JWT authentication)                  ║
// ║  3. Arrange tables (enable CORS for frontend)             ║
// ║  4. Open the doors (start the web server)                 ║
// ║                                                          ║
// ║  This file does ALL of that setup.                        ║
// ╚══════════════════════════════════════════════════════════╝

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StbCopilot.Api.Services;

// ═══════════════════════════════════════════════════════════
// STEP 1: Create the application builder
// This is like preparing the restaurant before opening.
// ═══════════════════════════════════════════════════════════
var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════
// STEP 2: Register our services (hire our employees)
//
// "AddSingleton" means: create ONE instance and share it everywhere.
// We use Singleton for RagService because we want ALL requests
// to share the same in-memory document storage.
//
// "AddScoped" means: create a new instance for each HTTP request.
// We use Scoped for CopilotService and AuthService.
// ═══════════════════════════════════════════════════════════
builder.Services.AddSingleton<RagService>();      // One shared RAG brain
builder.Services.AddScoped<CopilotService>();      // New per request
builder.Services.AddScoped<AuthService>();          // New per request

// ═══════════════════════════════════════════════════════════
// STEP 3: Set up JWT Authentication (security at the door)
//
// This tells .NET: "Check every request for a valid JWT token.
// If the token is missing or invalid, reject the request."
// ═══════════════════════════════════════════════════════════
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "StbCopilotDefaultSecretKey2026!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Verify the token was signed with our secret key.
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),

            // Verify the token was created by us (issuer = "StbCopilot").
            ValidateIssuer = true,
            ValidIssuer = "StbCopilot",

            // Verify the token is intended for our app.
            ValidateAudience = true,
            ValidAudience = "StbCopilotUsers",

            // Verify the token hasn't expired.
            ValidateLifetime = true
        };
    });

// ═══════════════════════════════════════════════════════════
// STEP 4: Enable CORS (Cross-Origin Resource Sharing)
//
// The Angular frontend runs on http://localhost:4200
// The .NET backend runs on http://localhost:5000
// By default, browsers BLOCK requests between different ports.
// CORS tells the browser: "It's OK, let the frontend talk to me."
// ═══════════════════════════════════════════════════════════
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add controller support (tells .NET to look for Controller classes).
builder.Services.AddControllers();

// ═══════════════════════════════════════════════════════════
// STEP 5: Build and configure the application
// ═══════════════════════════════════════════════════════════
var app = builder.Build();

// Enable CORS (must be before authentication).
app.UseCors("AllowFrontend");

// Enable authentication (JWT checking).
app.UseAuthentication();

// Enable authorization (role checking: Agent vs Admin).
app.UseAuthorization();

// Map all controllers to their routes.
app.MapControllers();

// ═══════════════════════════════════════════════════════════
// STEP 6: Start the server!
// The API will be available at http://localhost:5000
// ═══════════════════════════════════════════════════════════
Console.WriteLine("╔════════════════════════════════════════════╗");
Console.WriteLine("║  STB Copilot API is running!               ║");
Console.WriteLine("║  URL: http://localhost:5000                 ║");
Console.WriteLine("║  Login: POST /api/auth/login               ║");
Console.WriteLine("║  Ask:   POST /api/copilot/ask              ║");
Console.WriteLine("║  Docs:  POST /api/documents/upload         ║");
Console.WriteLine("╚════════════════════════════════════════════╝");

app.Run("http://localhost:5000");
