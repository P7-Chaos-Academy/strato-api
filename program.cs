using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using stratoapi.Data;
using stratoapi.Dtos;
using stratoapi.Middlewares;
using stratoapi.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure logging early so we can use it during startup
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Build a temporary logger for startup diagnostics
using var loggerFactory = LoggerFactory.Create(loggingBuilder =>
{
    loggingBuilder.AddConsole();
});
var startupLogger = loggerFactory.CreateLogger("Startup");

startupLogger.LogInformation("=== STRATO-API STARTING ===");
startupLogger.LogInformation("Environment: {Environment}", builder.Environment.EnvironmentName);
startupLogger.LogInformation("ContentRootPath: {Path}", builder.Environment.ContentRootPath);

// Load Docker secrets if running in container
LoadDockerSecrets(builder.Configuration, startupLogger);

// Configure Kestrel for Docker HTTPS
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(80); // HTTP
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        // Accept camelCase payloads from JS frontends (e.g. metricIds, startTime)
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Configure Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Log connection string with password masked
var maskedConnectionString = connectionString != null
    ? System.Text.RegularExpressions.Regex.Replace(connectionString, @"Password=([^;]*)", "Password=***")
    : "(not configured)";
startupLogger.LogInformation("Database Configuration - ConnectionString: {ConnectionString}", maskedConnectionString);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
           .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var issuer = jwtSettings["Issuer"] ?? "P7-Chaos-Academy";
var audience = jwtSettings["Audience"] ?? "clankers";

startupLogger.LogInformation("JWT Configuration - Issuer: {Issuer}, Audience: {Audience}, SecretKey length: {KeyLength}",
    issuer, audience, secretKey?.Length ?? 0);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add HttpContextAccessor for accessing user context in DbContext
builder.Services.AddHttpContextAccessor();

// Add authentication service
builder.Services.AddScoped<AuthService>();

// Api key service
builder.Services.AddSingleton<IApiKeyService, ApiKeyService>();

// Metrics service
builder.Services.AddScoped<IMetricsService, MetricsService>();

// Prometheus service
builder.Services.AddScoped<IPrometheusService, PrometheusService>();

// Job service
builder.Services.AddScoped<IJobService, JobService>();

// Cluster service
builder.Services.AddScoped<IClusterService, ClusterService>();

// Log API key configuration
var apiKeyHeaderName = builder.Configuration["ApiKeyHeaderName"] ?? "X-API-Key";
var apiKeyConfigured = !string.IsNullOrEmpty(builder.Configuration["ApiKey"]);
startupLogger.LogInformation("API Key Configuration - HeaderName: {HeaderName}, ApiKey configured: {Configured}",
    apiKeyHeaderName, apiKeyConfigured);


// Mappers
builder.Services.AddAutoMapper(typeof(MetricsDto).Assembly);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "stratoapi Backend API", Version = "v1" });
    
    // Add API Key Authentication to Swagger
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key needed to access the endpoints. X-API-Key: Your_API_Key",
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
            },
            new List<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    // Production policy - only allow the production frontend
    options.AddPolicy("Production",
        policy =>
        {
            policy.WithOrigins("https://frontend.gamel.dk")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });

    // Development policy - allow localhost origins
    options.AddPolicy("Development",
        policy =>
        {
            policy.WithOrigins(
                    "http://localhost:3000",
                    "http://127.0.0.1:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

startupLogger.LogInformation("=== APPLICATION BUILT - CONFIGURING PIPELINE ===");

// Use environment-specific CORS policy
var corsPolicy = app.Environment.IsDevelopment() ? "Development" : "Production";
startupLogger.LogInformation("Using CORS policy: {Policy}", corsPolicy);
app.UseCors(corsPolicy);

// Database migration and seeding
await RunMigrationsAsync(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

startupLogger.LogInformation("=== STRATO-API READY - STARTING HTTP SERVER ===");

app.Run();

static void LoadDockerSecrets(IConfiguration configuration, ILogger logger)
{
    const string secretsPath = "/run/secrets";

    if (Directory.Exists(secretsPath))
    {
        logger.LogInformation("Docker secrets directory found at {Path}", secretsPath);

        // Load database connection string from Docker secret
        var dbSecretPath = Path.Combine(secretsPath, "db_connection_string");
        if (File.Exists(dbSecretPath))
        {
            var connectionString = File.ReadAllText(dbSecretPath).Trim();
            ((IConfigurationBuilder)configuration).AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", connectionString)
            });
            logger.LogInformation("Loaded database connection string from Docker secret");
        }
        else
        {
            logger.LogDebug("No Docker secret found at {Path}", dbSecretPath);
        }
    }
    else
    {
        logger.LogDebug("Docker secrets directory not found at {Path} - using environment/appsettings configuration", secretsPath);
    }
}

async Task RunMigrationsAsync(WebApplication app)
{
    const int maxAttempts = 10;
    const int delayBetweenAttempts = 2000; // 2 seconds

    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            app.Logger.LogInformation("Attempting to connect to database and apply migrations (attempt {Attempt}/{MaxAttempts})", attempt, maxAttempts);
            
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            await context.Database.MigrateAsync();
            app.Logger.LogInformation("Database migrations applied successfully");
            return;
        }
        catch (Exception ex)
        {
            if (attempt == maxAttempts)
            {
                app.Logger.LogError(ex, "Failed to connect to database or apply migrations after {MaxAttempts} attempts", maxAttempts);
                throw;
            }
            
            app.Logger.LogWarning(ex, "Failed to connect to database or apply migrations (attempt {Attempt}/{MaxAttempts}). Retrying in {Delay} seconds...", 
                attempt, maxAttempts, delayBetweenAttempts / 1000);
            
            await Task.Delay(delayBetweenAttempts);
        }
    }
}
