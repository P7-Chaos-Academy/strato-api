using stratoapi.Common;

namespace stratoapi.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly IConfiguration _configuration;

    public ApiKeyService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsValidApiKey(string userApiKey)
    {
        if (string.IsNullOrWhiteSpace(userApiKey))
            return false;
        string? apiKey = _configuration.GetValue<string>(Constants.ApiKeyName);
        if (apiKey == null || apiKey != userApiKey)
            return false;
        return true;
    }
}