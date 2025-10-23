using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace stratoapi.Controllers;

public class ClusterController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ClusterController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("external-data")]
    public async Task<IActionResult> GetExternalData()
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync("https://raspberrypi.tailcaba77.ts.net/");
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, "Failed to fetch data");
        }
        var data = await response.Content.ReadAsStringAsync();
        return Ok("RaspberryPi said: \n" + data);
    }
}