using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using stratoapi.Dtos;
using stratoapi.Models;
using stratoapi.Services;

namespace stratoapi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{nameof(AuthRole.SeedUser)},{nameof(AuthRole.Admin)}")]
public class clusterController : ControllerBase
{
    private readonly IClusterService _clusterService;


    public clusterController(IClusterService clusterService)
    {
        _clusterService = clusterService;
    }

    [HttpGet("cluster")]
    public async Task<IActionResult> GetCluster()
    {
        List<Cluster> cluster = await _clusterService.GetAllClusters();
        return Ok(cluster);
    }
    
    [HttpPost("cluster")]
    public async Task<IActionResult> AddCluster([FromBody] ClusterDto cluster)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        await _clusterService.AddCluster(cluster);
        return Ok("Cluster added successfully.");
    }

    [HttpDelete("cluster/{id}")]
    public async Task<IActionResult> DeleteCluster(int id)
    {
        await _clusterService.DeleteCluster(id);
        return Ok("Cluster deleted successfully.");
    }

    [HttpPut("cluster/{id}")]
    public async Task<IActionResult> UpdateCluster(int id, [FromBody] ClusterDto cluster)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Cluster? updated = await _clusterService.UpdateCluster(id, cluster);
        if (updated == null)
        {
            return NotFound($"Cluster with ID {id} not found.");
        }

        return Ok(updated);
    }

    [HttpGet("health")]
    public async Task<IActionResult> CheckClusterHealth()
    {
        var healthStatuses = await _clusterService.CheckAllClustersHealth();
        return Ok(healthStatuses);
    }
}