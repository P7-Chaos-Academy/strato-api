using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using stratoapi.Dtos;
using stratoapi.Models;
using stratoapi.Services;

namespace stratoapi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{nameof(AuthRole.SeedUser)},{nameof(AuthRole.Admin)}")]
public class MetricsController : ControllerBase
{
    private readonly IMetricsService _metricsService;
    private readonly IPrometheusService _prometheusService;


    public MetricsController(IMetricsService metricsService, IPrometheusService prometheusService)
    {
        _metricsService = metricsService;
        _prometheusService = prometheusService;
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        List<MetricType> metrics = await _metricsService.GetAllMetricTypes();
        return Ok(metrics);
    }
    
    [HttpPost("metrics")]
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
    public async Task<IActionResult> DeleteMetric(int id)
    {
        await _metricsService.DeleteMetricType(id);
        return Ok("Metric type deleted successfully.");
    }

    [HttpPut("metrics/{id}")]
    public async Task<IActionResult> UpdateMetric(int id, [FromBody] MetricsDto metricType)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        MetricType? updated = await _metricsService.UpdateMetricType(id, metricType);
        if (updated == null)
        {
            return NotFound($"Metric type with ID {id} not found.");
        }

        return Ok(updated);
    }
    
    /// <summary>
    /// Simplified proxy to Prometheus. Does not require JWT but does require the API key middleware.
    /// Use this endpoint to execute instant or range queries against the configured Prometheus server.
    /// </summary>
    [HttpPost("query")]
    public async Task<IActionResult> QueryPrometheus([FromBody] PrometheusQueryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        string result = await _prometheusService.QueryAsync(dto);
        return Content(result, "application/json");
    }
}