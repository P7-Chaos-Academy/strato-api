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
    public async Task<IActionResult> Getcluster()
    {
        List<Cluster> cluster = await _clusterService.GetAllClusters();
        return Ok(cluster);
    }
    
    [HttpPost("cluster")]
    public async Task<IActionResult> AddMetric([FromBody] ClusterDto cluster)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        await _clusterService.AddCluster(cluster);
        return Ok("Metric type added successfully.");
    }

    [HttpDelete("cluster/{id}")]
    public async Task<IActionResult> DeleteMetric(int id)
    {
        await _clusterService.DeleteCluster(id);
        return Ok("Metric type deleted successfully.");
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
            return NotFound($"Metric type with ID {id} not found.");
        }

        return Ok(updated);
    }
}