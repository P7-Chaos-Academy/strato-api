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

    public ApiKeyMiddleware(RequestDelegate next, IApiKeyService apiKeyValidation, IConfiguration configuration)
    {
        _next = next;
        _apiKeyValidation = apiKeyValidation;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headerName = _configuration.GetValue<string>("ApiKeyHeaderName") ?? Constants.ApiKeyHeaderName;

        if (!context.Request.Headers.TryGetValue(headerName, out var extractedApiKey) || string.IsNullOrWhiteSpace(extractedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("API key missing");
            return;
        }

        string userApiKey = extractedApiKey.ToString();

        if (!_apiKeyValidation.IsValidApiKey(userApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Invalid API key");
            return;
        }

        await _next(context);
    }
}