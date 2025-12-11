using Microsoft.AspNetCore.Mvc;

namespace stratoapi.Helpers;

public class HttpClientHelper
{
    private readonly ILogger<HttpClientHelper> _logger;
    public HttpClientHelper(ILogger<HttpClientHelper> logger)
    {
        _logger = logger;
    }
    private HttpClient HttpClientFactory(string clusterBaseUrl)
    {
        return new HttpClient()
        {
            BaseAddress = new Uri(clusterBaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<IActionResult> HttpClient(string clusterBaseUrl, string uri, HttpMethod method, HttpContent? content = null)
    {
        _logger.LogInformation("Making {Method} request to cluster at {BaseUrl}/{Uri}", method, clusterBaseUrl, uri);

        HttpClient client;
        try
        {
            client = HttpClientFactory(clusterBaseUrl);
        }
        catch (UriFormatException ex)
        {
            _logger.LogWarning("Invalid cluster URL {ClusterBaseUrl}: {Message}", clusterBaseUrl, ex.Message);
            return new BadRequestObjectResult(new { error = "Invalid cluster URL", clusterBaseUrl, details = ex.Message });
        }

        HttpRequestMessage request = new HttpRequestMessage(method, uri)
        {
            Content = content
        };

        HttpResponseMessage res;
        try
        {
            res = await client.SendAsync(request);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to cluster API at {BaseAddress}{Uri}: {Message}",
                client.BaseAddress, uri, ex.Message);
            return new ObjectResult(new {
                error = "Failed to connect to cluster API",
                clusterBaseUrl,
                details = ex.Message
            }) { StatusCode = 502 };
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Request to cluster at {BaseAddress}{Uri} timed out", client.BaseAddress, uri);
            return new ObjectResult(new {
                error = "Request to cluster API timed out",
                clusterBaseUrl
            }) { StatusCode = 504 };
        }

        if (!res.IsSuccessStatusCode)
        {
            string errorContent = await res.Content.ReadAsStringAsync();
            _logger.LogWarning("Cluster at {BaseAddress} returned {StatusCode} for {Method} {Uri}: {ErrorContent}",
                client.BaseAddress, (int)res.StatusCode, method, uri, errorContent);

            return new ObjectResult(new {
                error = $"Cluster API returned {(int)res.StatusCode}",
                clusterBaseUrl,
                statusCode = (int)res.StatusCode,
                details = errorContent
            }) { StatusCode = (int)res.StatusCode };
        }

        string responseContent = await res.Content.ReadAsStringAsync();
        _logger.LogDebug("Response content from cluster at {BaseAddress}: {ResponseContent}", client.BaseAddress, responseContent);

        return new OkObjectResult(responseContent);
    }
}