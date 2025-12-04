using Microsoft.AspNetCore.Mvc;
using stratoapi.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace stratoapi.Services;

public class JobService : IJobService
{
    private readonly HttpClient _httpClient;
    
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions 
    { 
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public JobService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IActionResult> PostJob(JobRequestDto dto)
    {
        var requestBody = new { dto.prompt, dto.n_predict, dto.temperature };
        JsonContent content = JsonContent.Create(requestBody);
        string uriPrefix = "api/v1/jobs/";
        HttpResponseMessage res = await _httpClient.PostAsync(uriPrefix, content);
        
        if (!res.IsSuccessStatusCode)
        {
            string errorContent = await res.Content.ReadAsStringAsync();
            return new BadRequestObjectResult(new { error = errorContent, statusCode = res.StatusCode });
        }
        
        string responseContent = await res.Content.ReadAsStringAsync();
        JobResponseDto? jobResponse = JsonSerializer.Deserialize<JobResponseDto>(responseContent, JsonOptions);
        return new OkObjectResult(jobResponse);
    }

    public async Task<IActionResult> GetEstimatedTimeRemaining(int tokenCount)
    {
        string uri = "/api/v1/nodes/all-node-speeds";
        HttpResponseMessage res = await _httpClient.GetAsync(uri);
        
        if (!res.IsSuccessStatusCode)
        {
            string errorContent = await res.Content.ReadAsStringAsync();
            return new BadRequestObjectResult(new { error = errorContent, statusCode = res.StatusCode });
        }
        
        string responseContent = await res.Content.ReadAsStringAsync();
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
        }
        
        return new OkObjectResult(new { estimatedTimeRemainingSeconds = estimatedTime });
    }
    
    public async Task<IActionResult> GetJobStatus(string jobId)
    {
        string uri = $"/api/v1/jobs/history/{jobId}";
        HttpResponseMessage res = await _httpClient.GetAsync(uri);
        
        if (!res.IsSuccessStatusCode)
        {
            string errorContent = await res.Content.ReadAsStringAsync();
            return new BadRequestObjectResult(new { error = errorContent, statusCode = res.StatusCode });
        }
        
        string responseContent = await res.Content.ReadAsStringAsync();
        JobStatusResponseDto? jobStatusResponse = JsonSerializer.Deserialize<JobStatusResponseDto>(responseContent, JsonOptions);
        
        return new OkObjectResult(jobStatusResponse);
    }
}