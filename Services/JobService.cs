using Microsoft.AspNetCore.Mvc;
using stratoapi.Dtos;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace stratoapi.Services;

public class JobService : IJobService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<JobService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public JobService(HttpClient httpClient, ILogger<JobService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _logger.LogInformation("[JobService] Initialized with base address: {BaseAddress}", _httpClient.BaseAddress);
    }

    public async Task<IActionResult> PostJob(JobRequestDto dto)
    {
        var sw = Stopwatch.StartNew();
        var requestBody = new { dto.prompt, dto.n_predict, dto.temperature };
        JsonContent content = JsonContent.Create(requestBody);
        string uriPrefix = "api/v1/jobs/";

        _logger.LogInformation("[JobService] PostJob - Sending request to {Uri} with n_predict={TokenCount}, temperature={Temperature}",
            _httpClient.BaseAddress + uriPrefix, dto.n_predict, dto.temperature);

        try
        {
            HttpResponseMessage res = await _httpClient.PostAsync(uriPrefix, content);
            sw.Stop();

            if (!res.IsSuccessStatusCode)
            {
                string errorContent = await res.Content.ReadAsStringAsync();
                _logger.LogError("[JobService] PostJob failed - Status: {StatusCode}, Duration: {Duration}ms, Error: {Error}",
                    (int)res.StatusCode, sw.ElapsedMilliseconds, errorContent);
                return new BadRequestObjectResult(new { error = errorContent, statusCode = res.StatusCode });
            }

            string responseContent = await res.Content.ReadAsStringAsync();
            _logger.LogInformation("[JobService] PostJob succeeded - Status: {StatusCode}, Duration: {Duration}ms",
                (int)res.StatusCode, sw.ElapsedMilliseconds);
            _logger.LogDebug("[JobService] PostJob response: {Response}", responseContent);

            JobResponseDto? jobResponse = JsonSerializer.Deserialize<JobResponseDto>(responseContent, JsonOptions);
            return new OkObjectResult(jobResponse);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "[JobService] PostJob exception after {Duration}ms - Target: {Uri}",
                sw.ElapsedMilliseconds, _httpClient.BaseAddress + uriPrefix);
            throw;
        }
    }

    public async Task<IActionResult> GetEstimatedTimeRemaining(int tokenCount)
    {
        var sw = Stopwatch.StartNew();
        string uri = "/api/v1/nodes/all-node-speeds";

        _logger.LogInformation("[JobService] GetEstimatedTimeRemaining - Fetching node speeds from {Uri} for {TokenCount} tokens",
            _httpClient.BaseAddress + uri, tokenCount);

        try
        {
            HttpResponseMessage res = await _httpClient.GetAsync(uri);
            sw.Stop();

            if (!res.IsSuccessStatusCode)
            {
                string errorContent = await res.Content.ReadAsStringAsync();
                _logger.LogError("[JobService] GetEstimatedTimeRemaining failed - Status: {StatusCode}, Duration: {Duration}ms, Error: {Error}",
                    (int)res.StatusCode, sw.ElapsedMilliseconds, errorContent);
                return new BadRequestObjectResult(new { error = errorContent, statusCode = res.StatusCode });
            }

            string responseContent = await res.Content.ReadAsStringAsync();
            _logger.LogDebug("[JobService] GetEstimatedTimeRemaining response: {Response}", responseContent);

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

                _logger.LogInformation("[JobService] GetEstimatedTimeRemaining - NodeCount: {NodeCount}, AvgSpeed: {AvgSpeed}, EstimatedTime: {EstimatedTime}s",
                    nodeSpeedsResponse.NodeSpeeds.Count, averageSpeed, estimatedTime);
            }
            else
            {
                _logger.LogWarning("[JobService] GetEstimatedTimeRemaining - No node speeds available");
            }

            return new OkObjectResult(new { estimatedTimeRemainingSeconds = estimatedTime });
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "[JobService] GetEstimatedTimeRemaining exception after {Duration}ms - Target: {Uri}",
                sw.ElapsedMilliseconds, _httpClient.BaseAddress + uri);
            throw;
        }
    }

    public async Task<IActionResult> GetJobStatus(string jobId)
    {
        var sw = Stopwatch.StartNew();
        string uri = $"/api/v1/jobs/history/{jobId}";

        _logger.LogInformation("[JobService] GetJobStatus - Fetching status for job {JobId} from {Uri}",
            jobId, _httpClient.BaseAddress + uri);

        try
        {
            HttpResponseMessage res = await _httpClient.GetAsync(uri);
            sw.Stop();

            if (!res.IsSuccessStatusCode)
            {
                string errorContent = await res.Content.ReadAsStringAsync();
                _logger.LogError("[JobService] GetJobStatus failed for {JobId} - Status: {StatusCode}, Duration: {Duration}ms, Error: {Error}",
                    jobId, (int)res.StatusCode, sw.ElapsedMilliseconds, errorContent);
                return new BadRequestObjectResult(new { error = errorContent, statusCode = res.StatusCode });
            }

            string responseContent = await res.Content.ReadAsStringAsync();
            _logger.LogInformation("[JobService] GetJobStatus succeeded for {JobId} - Duration: {Duration}ms",
                jobId, sw.ElapsedMilliseconds);
            _logger.LogDebug("[JobService] GetJobStatus response: {Response}", responseContent);

            JobStatusResponseDto? jobStatusResponse = JsonSerializer.Deserialize<JobStatusResponseDto>(responseContent, JsonOptions);

            return new OkObjectResult(jobStatusResponse);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "[JobService] GetJobStatus exception for {JobId} after {Duration}ms",
                jobId, sw.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<IActionResult> GetAllJobs()
    {
        var sw = Stopwatch.StartNew();
        string uri = "/api/v1/jobs/status";

        _logger.LogInformation("[JobService] GetAllJobs - Fetching all jobs from {Uri}", _httpClient.BaseAddress + uri);

        try
        {
            HttpResponseMessage res = await _httpClient.GetAsync(uri);
            sw.Stop();

            if (!res.IsSuccessStatusCode)
            {
                string errorContent = await res.Content.ReadAsStringAsync();
                _logger.LogError("[JobService] GetAllJobs failed - Status: {StatusCode}, Duration: {Duration}ms, Error: {Error}",
                    (int)res.StatusCode, sw.ElapsedMilliseconds, errorContent);
                return new BadRequestObjectResult(new { error = errorContent, statusCode = res.StatusCode });
            }

            string responseContent = await res.Content.ReadAsStringAsync();
            _logger.LogInformation("[JobService] GetAllJobs succeeded - Status: {StatusCode}, Duration: {Duration}ms",
                (int)res.StatusCode, sw.ElapsedMilliseconds);
            _logger.LogDebug("[JobService] GetAllJobs response: {Response}", responseContent);

            AllJobsResponseDto? allJobsResponse = JsonSerializer.Deserialize<AllJobsResponseDto>(responseContent, JsonOptions);

            return new OkObjectResult(allJobsResponse);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "[JobService] GetAllJobs exception after {Duration}ms - Target: {Uri}",
                sw.ElapsedMilliseconds, _httpClient.BaseAddress + uri);
            throw;
        }
    }
}