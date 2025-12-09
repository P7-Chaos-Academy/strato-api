using Microsoft.AspNetCore.Mvc;
using stratoapi.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace stratoapi.Services;

public class JobService : IJobService
{
    private readonly IClusterService _clusterService;
    private readonly ILogger<JobService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public JobService(IClusterService clusterService, ILogger<JobService> logger)
    {
        _clusterService = clusterService;
        _logger = logger;
    }

    public async Task<IActionResult> PostJob(JobRequestDto dto)
    {
        _logger.LogInformation("PostJob called for cluster {ClusterId} with prompt length {PromptLength}",
            dto.ClusterId, dto.prompt?.Length ?? 0);

        var requestBody = new { dto.prompt, dto.n_predict, dto.temperature };
        JsonContent content = JsonContent.Create(requestBody);
        string uri = "jobs/";

        IActionResult result = await HttpClientHelper(dto.ClusterId, uri, HttpMethod.Post, content);

        if (result is OkObjectResult okResult && okResult.Value is string responseContent)
        {
            JobResponseDto? jobResponse = JsonSerializer.Deserialize<JobResponseDto>(responseContent, JsonOptions);
            _logger.LogInformation("Job created successfully: {JobName}", jobResponse?.JobName);
            return new OkObjectResult(jobResponse);
        }

        return result;
    }

    public async Task<IActionResult> GetEstimatedTimeRemaining(int tokenCount, int clusterId)
    {
        _logger.LogInformation("GetEstimatedTimeRemaining called for cluster {ClusterId} with {TokenCount} tokens",
            clusterId, tokenCount);

        string uri = "nodes/all-node-speeds";

        IActionResult result = await HttpClientHelper(clusterId, uri, HttpMethod.Get);

        if (result is OkObjectResult okResult && okResult.Value is string responseContent)
        {
            NodeSpeedsResponseDto? nodeSpeedsResponse = JsonSerializer.Deserialize<NodeSpeedsResponseDto>(responseContent, JsonOptions);

            float estimatedTime = 0;
            if (nodeSpeedsResponse != null && nodeSpeedsResponse.NodeSpeeds.Count > 0)
            {
                double totalSpeed = 0;
                foreach (var speed in nodeSpeedsResponse.NodeSpeeds.Values)
                {
                    totalSpeed += speed;
                }
                double averageSpeed = totalSpeed / nodeSpeedsResponse.NodeSpeeds.Count;
                estimatedTime = (float)(tokenCount / averageSpeed);
                _logger.LogInformation("Estimated time for {TokenCount} tokens: {EstimatedTime}s (avg speed: {AvgSpeed})",
                    tokenCount, estimatedTime, averageSpeed);
            }

            return new OkObjectResult(new { estimatedTimeRemainingSeconds = estimatedTime });
        }

        return result;
    }

    public async Task<IActionResult> GetJobStatus(string jobId, int clusterId)
    {
        _logger.LogInformation("GetJobStatus called for job {JobId} on cluster {ClusterId}", jobId, clusterId);

        string uri = $"jobs/history/{jobId}";

        IActionResult result = await HttpClientHelper(clusterId, uri, HttpMethod.Get);

        if (result is OkObjectResult okResult && okResult.Value is string responseContent)
        {
            JobStatusResponseDto? jobStatusResponse = JsonSerializer.Deserialize<JobStatusResponseDto>(responseContent, JsonOptions);
            _logger.LogInformation("Job {JobId} status: {Status}", jobId, jobStatusResponse?.Status);
            return new OkObjectResult(jobStatusResponse);
        }

        return result;
    }

    public async Task<IActionResult> GetAllJobs(int clusterId)
    {
        _logger.LogInformation("GetAllJobs called for cluster {ClusterId}", clusterId);

        string uri = "jobs/status";

        IActionResult result = await HttpClientHelper(clusterId, uri, HttpMethod.Get);

        if (result is OkObjectResult okResult && okResult.Value is string responseContent)
        {
            AllJobsResponseDto? allJobsResponse = JsonSerializer.Deserialize<AllJobsResponseDto>(responseContent, JsonOptions);
            _logger.LogInformation("Retrieved {JobCount} jobs from cluster {ClusterId}",
                allJobsResponse?.Jobs?.Count ?? 0, clusterId);
            return new OkObjectResult(allJobsResponse);
        }

        return result;
    }

    private async Task<HttpClient> HttpClientFactory(int clusterId)
    {
        string clusterBaseUrl = await _clusterService.GetClusterApiEndpoint(clusterId);

        return new HttpClient()
        {
            BaseAddress = new Uri(clusterBaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    private async Task<IActionResult> HttpClientHelper(int clusterId, string uri, HttpMethod method, HttpContent? content = null)
    {
        _logger.LogInformation("Making {Method} request to cluster {ClusterId} at {Uri}", method, clusterId, uri);

        HttpClient client;
        try
        {
            client = await HttpClientFactory(clusterId);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Cluster {ClusterId} not found: {Message}", clusterId, ex.Message);
            return new NotFoundObjectResult(new { error = ex.Message, clusterId });
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
            _logger.LogError(ex, "Failed to connect to cluster {ClusterId} API at {BaseAddress}{Uri}: {Message}",
                clusterId, client.BaseAddress, uri, ex.Message);
            return new ObjectResult(new {
                error = "Failed to connect to cluster API",
                clusterId,
                details = ex.Message
            }) { StatusCode = 502 };
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Request to cluster {ClusterId} at {Uri} timed out", clusterId, uri);
            return new ObjectResult(new {
                error = "Request to cluster API timed out",
                clusterId
            }) { StatusCode = 504 };
        }

        if (!res.IsSuccessStatusCode)
        {
            string errorContent = await res.Content.ReadAsStringAsync();
            _logger.LogWarning("Cluster {ClusterId} returned {StatusCode} for {Method} {BaseAddress}{Uri}: {ErrorContent}",
                clusterId, (int)res.StatusCode, method, client.BaseAddress, uri, errorContent);

            return new ObjectResult(new {
                error = $"Cluster API returned {(int)res.StatusCode}",
                clusterId,
                statusCode = (int)res.StatusCode,
                details = errorContent
            }) { StatusCode = (int)res.StatusCode };
        }

        string responseContent = await res.Content.ReadAsStringAsync();
        _logger.LogDebug("Response content from cluster {ClusterId}: {ResponseContent}", clusterId, responseContent);

        return new OkObjectResult(responseContent);
    }
}
