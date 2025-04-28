using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AngularApp1.Server.Services;
using AngularApp1.Server.Repositories;
using AngularApp1.Server.Repositories.Interfaces;
using AngularApp1.Server.Configurations;
using MongoDB.Driver;
using AngularApp1.Server.Models.Entities;
using System.Net.WebSockets;
using Microsoft.Extensions.Options;
using AngularApp1.Server.Services.Interfaces;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddSwaggerGen();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBotConfigService, BotConfigService>();
builder.Services.AddScoped<IDialogflowService, DialogflowService>();
builder.Services.AddScoped<IBotConfigRepository, BotConfigRepository>();
builder.Services.AddScoped<IBotConfigService, BotConfigService>();

// Configure Google Dialogflow
var credentialsPath = Path.Combine(AppContext.BaseDirectory, "dialogflow-key.json");
System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

// Configure MongoDB
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoSettings"));
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
    new MongoClient(serviceProvider.GetRequiredService<IOptions<MongoSettings>>().Value.ConnectionString));
builder.Services.AddScoped<IMongoDatabase>(serviceProvider =>
    serviceProvider.GetRequiredService<IMongoClient>()
        .GetDatabase(serviceProvider.GetRequiredService<IOptions<MongoSettings>>().Value.DatabaseName));
builder.Services.AddScoped<IMongoCollection<BotConfig>>(serviceProvider =>
    serviceProvider.GetRequiredService<IMongoDatabase>().GetCollection<BotConfig>("BotConfigs"));

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Headers["Authorization"]
                    .FirstOrDefault()?.Split(" ").Last()
                    ?? context.Request.Query["access_token"];

                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Configure CORS
var allowedOrigins = new[]
{
    "http://localhost:8080"
};
var corsPolicyName = "AllowFrontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Middleware pipeline
app.UseRouting();
app.UseCors(corsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

// WebSocket configuration - Removed AllowedOrigins as it's read-only
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120)
});

// WebSocket endpoint with manual origin check
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var buffer = new byte[1024 * 4];

        // Inject DialogflowService (assuming DI is set up)
        var dialogflowService = context.RequestServices.GetRequiredService<IDialogflowService>();

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }
                else if (result.Count > 0)
                {
                    string userMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // Send to Dialogflow
                    string sessionId = "unique-session-id";
                    var dfResponse = await dialogflowService.DetectIntentWithDetailsAsync(sessionId, userMessage);

                    // with this:
                    var jsonPayload = JsonSerializer.Serialize(
                        dfResponse,
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }
                    );

                    // Send one WebSocket frame containing the full JSON
                    var responseBytes = Encoding.UTF8.GetBytes(jsonPayload);
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(responseBytes),
                        WebSocketMessageType.Text,
                        endOfMessage: true,
                        CancellationToken.None
                    );
                }
            }
        }
        catch (Exception ex)
        {
            if (webSocket.State == WebSocketState.Open)
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error occurred", CancellationToken.None);
        }
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

// Controllers and Swagger
app.MapControllers();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Startup logging
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot/browser")),
    RequestPath = ""
});


// Handle client-side routing
app.MapFallbackToFile("index.html", new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot/browser"))
});

app.Run();

// WebSocket handler
static async Task HandleWebSocketConnection(
    WebSocket webSocket,
    IDialogflowService dialogflowService,
    ClaimsPrincipal user)
{
    var buffer = new byte[1024 * 4];
    try
    {
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        while (!result.CloseStatus.HasValue)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
            var response = await dialogflowService.DetectIntentAsync(userId, message);

            var responseBytes = Encoding.UTF8.GetBytes(response);
            await webSocket.SendAsync(
                new ArraySegment<byte>(responseBytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);

            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(
            result.CloseStatus.Value,
            result.CloseStatusDescription ?? "Normal closure",
            CancellationToken.None);
    }
    catch (Exception ex)
    {

        if (webSocket.State == WebSocketState.Open)
        {
            await webSocket.CloseAsync(
                WebSocketCloseStatus.InternalServerError,
                "Connection closed due to error",
                CancellationToken.None);
        }
    }
}