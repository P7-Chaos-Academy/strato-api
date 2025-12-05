using System.Net;
using Microsoft.Extensions.Configuration;
using stratoapi.Common;
using stratoapi.Services;

namespace stratoapi.Middlewares;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IApiKeyService _apiKeyValidation;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(RequestDelegate next, IApiKeyService apiKeyValidation, IConfiguration configuration, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _apiKeyValidation = apiKeyValidation;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string headerName = _configuration.GetValue<string>("ApiKeyHeaderName") ?? Constants.ApiKeyHeaderName;
        var method = context.Request.Method;
        var path = context.Request.Path;
        var remoteIp = context.Connection.RemoteIpAddress;

        _logger.LogDebug("[ApiKey] {Method} {Path} from {RemoteIp}", method, path, remoteIp);

        if (path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWithSegments("/swagger-ui", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("[ApiKey] Skipping API key check for swagger path: {Path}", path);
            await _next(context);
            return;
        }

        // Skip API key check for CORS preflight requests
        if (context.Request.Method == HttpMethods.Options)
        {
            _logger.LogDebug("[ApiKey] Skipping API key check for OPTIONS preflight request: {Path}", path);
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(headerName, out var extractedApiKey) || string.IsNullOrWhiteSpace(extractedApiKey))
        {
            _logger.LogWarning("[ApiKey] API key missing for {Method} {Path} from {RemoteIp}. Expected header: {HeaderName}",
                method, path, remoteIp, headerName);
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("API key missing");
            return;
        }

        string userApiKey = extractedApiKey.ToString();
        var maskedKey = userApiKey.Length > 10 ? userApiKey[..10] + "..." : "***";

        if (!_apiKeyValidation.IsValidApiKey(userApiKey))
        {
            _logger.LogWarning("[ApiKey] Invalid API key provided for {Method} {Path} from {RemoteIp}. Key prefix: {MaskedKey}",
                method, path, remoteIp, maskedKey);
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Invalid API key");
            return;
        }

        _logger.LogDebug("[ApiKey] API key validated successfully for {Method} {Path}", method, path);
        await _next(context);
    }
}