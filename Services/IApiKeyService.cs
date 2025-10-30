namespace stratoapi.Services;

public interface IApiKeyService
{
    bool IsValidApiKey(string userApiKey);
}