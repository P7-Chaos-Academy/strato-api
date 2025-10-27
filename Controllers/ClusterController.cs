using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stratoapi.Dtos;
using stratoapi.Models;
using stratoapi.Services;

namespace stratoapi.Controllers;

public class ClusterController : ControllerBase
{
    private readonly ClusterService _clusterService;

    public ClusterController(ClusterService clusterService)
    {
        _clusterService = clusterService;
    }

    [HttpGet("cluster/metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        
        var result = await _clusterService.GetMetrics();
        // TODO: Handle more specific error cases
        if (result == (false, null))
        {
            return BadRequest("Failed to retrieve metrics from cluster.");
        }
        
        return Ok(result);
    }
    
    [HttpGet("cluster/shutdown/{nodeId}")]
    [Authorize(Roles = $"{nameof(AuthRole.Admin)}")]
    public async Task<string> ShutdownNode(int nodeId)
    {
        return await _clusterService.ShutdownNode(nodeId);
    }

    [HttpGet("cluster/startup/{nodeId}")]
    [Authorize(Roles = $"{nameof(AuthRole.Admin)}")]
    public async Task<string> StartupNode(int nodeId)
    {
        return await _clusterService.StartupNode(nodeId);
    }
    
    [HttpGet("cluster/runtask/{jobId}")]
    public async Task<IActionResult> RunTask(int jobId)
    {
        return await _clusterService.RunTask(jobId);
    }
}