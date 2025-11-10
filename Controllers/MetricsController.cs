using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using stratoapi.Dtos;
using stratoapi.Models;
using stratoapi.Services;

namespace stratoapi.Controllers;

public class MetricsController : ControllerBase
{
    private readonly IMetricsService _metricsService;
    

    public MetricsController(IMetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    [HttpGet("metrics")]
    [Authorize(Roles = $"{nameof(AuthRole.SeedUser)},{nameof(AuthRole.Admin)}")]
    public async Task<IActionResult> GetMetrics()
    {
        var metrics = await _metricsService.GetAllMetricTypes();
        return Ok(metrics);
    }
    
    [HttpPost("metrics")]
    [Authorize(Roles = $"{nameof(AuthRole.SeedUser)},{nameof(AuthRole.Admin)}")]

    public async Task<IActionResult> AddMetric([FromBody] MetricsDto metricType)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        await _metricsService.AddMetricType(metricType);
        return Ok("Metric type added successfully.");
    }

    [HttpDelete("metrics/{id}")]
    [Authorize(Roles = $"{nameof(AuthRole.SeedUser)},{nameof(AuthRole.Admin)}")]
    public async Task<IActionResult> DeleteMetric(int id)
    {
        await _metricsService.DeleteMetricType(id);
        return Ok("Metric type deleted successfully.");
    }
}