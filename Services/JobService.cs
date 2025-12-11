using Microsoft.AspNetCore.Mvc;
using stratoapi.Dtos;
using stratoapi.Helpers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace stratoapi.Services;

public class JobService : IJobService
{
    private readonly IClusterService _clusterService;
    private readonly ILogger<JobService> _logger;
    private readonly HttpClientHelper _httpClientHelper;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public JobService(IClusterService clusterService, ILogger<JobService> logger, HttpClientHelper httpClientHelper)
    {
        _clusterService = clusterService;
        _logger = logger;
        _httpClientHelper = httpClientHelper;
    }

    public async Task<IActionResult> PostJob(JobRequestDto dto)
    {
        _logger.LogInformation("PostJob called for cluster {ClusterId} with prompt length {PromptLength}",
            dto.ClusterId, dto.prompt?.Length ?? 0);

        string clusterApiEndpoint;
        try
        {
            clusterApiEndpoint = await _clusterService.GetClusterApiEndpoint(dto.ClusterId);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Cluster {ClusterId} not found: {Message}", dto.ClusterId, ex.Message);
            return new NotFoundObjectResult(new { error = ex.Message, clusterId = dto.ClusterId });
        }

        var requestBody = new { dto.prompt, dto.n_predict, dto.temperature };
        JsonContent content = JsonContent.Create(requestBody);
        string uri = "jobs/";

        IActionResult result = await _httpClientHelper.HttpClient(clusterApiEndpoint, uri, HttpMethod.Post, content);

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

        string clusterApiEndpoint;
        try
        {
            clusterApiEndpoint = await _clusterService.GetClusterApiEndpoint(clusterId);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Cluster {ClusterId} not found: {Message}", clusterId, ex.Message);
            return new NotFoundObjectResult(new { error = ex.Message, clusterId });
        }

        string uri = "nodes/all-node-speeds";

        IActionResult result = await  _httpClientHelper.HttpClient(clusterApiEndpoint, uri, HttpMethod.Get);

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

        string clusterApiEndpoint;
        try
        {
            clusterApiEndpoint = await _clusterService.GetClusterApiEndpoint(clusterId);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Cluster {ClusterId} not found: {Message}", clusterId, ex.Message);
            return new NotFoundObjectResult(new { error = ex.Message, clusterId });
        }

        string uri = $"jobs/history/{jobId}";

        IActionResult result = await  _httpClientHelper.HttpClient(clusterApiEndpoint, uri, HttpMethod.Get);

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

        string clusterApiEndpoint;
        try
        {
            clusterApiEndpoint = await _clusterService.GetClusterApiEndpoint(clusterId);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Cluster {ClusterId} not found: {Message}", clusterId, ex.Message);
            return new NotFoundObjectResult(new { error = ex.Message, clusterId });
        }

        string uri = "jobs/status";

        IActionResult result = await  _httpClientHelper.HttpClient(clusterApiEndpoint, uri, HttpMethod.Get);

        if (result is OkObjectResult okResult && okResult.Value is string responseContent)
        {
            AllJobsResponseDto? allJobsResponse = JsonSerializer.Deserialize<AllJobsResponseDto>(responseContent, JsonOptions);
            _logger.LogInformation("Retrieved {JobCount} jobs from cluster {ClusterId}",
                allJobsResponse?.Jobs?.Count ?? 0, clusterId);
            return new OkObjectResult(allJobsResponse);
        }

        return result;
    }
}
