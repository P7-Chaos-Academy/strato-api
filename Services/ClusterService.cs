using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using stratoapi.Dtos;
namespace stratoapi.Services;

public class ClusterService
{
    private readonly IHttpClientFactory _httpClient;
    private string raspURL = "https://raspberrypi.tailcaba77.ts.net/";
    
    public ClusterService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory;
    }
    private HttpClient HttpClient => _httpClient.CreateClient();
    
    public async Task<(bool Success, MetricsDto? Data)> GetMetrics()
    {
        var client = HttpClient;
        var response = await client.GetAsync(raspURL);
        if (!response.IsSuccessStatusCode)
        {
            return (false, null);
        }

        var data = await response.Content.ReadFromJsonAsync<MetricsDto>();
        return (true, data);
    }
    
    public Task<string> ShutdownNode(int nodeId)
    {
        var client = HttpClient;
        var response = client.GetAsync(raspURL + $"/cluster/shutdown/{nodeId}");
        if (!response.Result.IsSuccessStatusCode)
        {
            return Task.FromResult("Failed to send shutdown command to node " + nodeId);
        }
        
        return Task.FromResult("Shutdown command sent to node " + nodeId);
    }
    
    public Task<string> StartupNode(int nodeId)
    {
        var client = HttpClient;
        var response = client.GetAsync(raspURL + $"/cluster/startup/{nodeId}");
        if (!response.Result.IsSuccessStatusCode)
        {
            return Task.FromResult("Failed to send startup command to node " + nodeId);
        }
        
        return Task.FromResult("Startup command sent to node " + nodeId);
    }
    
    public async Task<IActionResult> RunTask(int jobId)
    {
        var client = HttpClient;
        var response = client.GetAsync(raspURL + $"/cluster/runJob/{jobId}");
        if (!response.Result.IsSuccessStatusCode)
        {
            return Task.FromResult("Job Could not be completed") as IActionResult;
        }

        return new OkObjectResult("Task completed successfully");
    }
}