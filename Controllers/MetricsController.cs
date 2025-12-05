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
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(IMetricsService metricsService, IPrometheusService prometheusService, ILogger<MetricsController> logger)
    {
        _metricsService = metricsService;
        _prometheusService = prometheusService;
        _logger = logger;
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        var currentUser = User.Identity?.Name ?? "unknown";
        _logger.LogInformation("[MetricsController] GET /api/Metrics/metrics - User: {User}", currentUser);

        List<MetricType> metrics = await _metricsService.GetAllMetricTypes();
        _logger.LogDebug("[MetricsController] GetMetrics returned {Count} metric types", metrics.Count);

        return Ok(metrics);
    }

    [HttpPost("metrics")]
    public async Task<IActionResult> AddMetric([FromBody] MetricsDto metricType)
    {
        var currentUser = User.Identity?.Name ?? "unknown";
        _logger.LogInformation("[MetricsController] POST /api/Metrics/metrics - User: {User}, MetricName: {Name}",
            currentUser, metricType.Name);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[MetricsController] AddMetric validation failed: {Errors}",
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(ModelState);
        }

        await _metricsService.AddMetricType(metricType);
        _logger.LogInformation("[MetricsController] Metric type added successfully: {Name}", metricType.Name);

        return Ok("Metric type added successfully.");
    }

    [HttpDelete("metrics/{id}")]
    public async Task<IActionResult> DeleteMetric(int id)
    {
        var currentUser = User.Identity?.Name ?? "unknown";
        _logger.LogInformation("[MetricsController] DELETE /api/Metrics/metrics/{Id} - User: {User}", id, currentUser);

        try
        {
            await _metricsService.DeleteMetricType(id);
            _logger.LogInformation("[MetricsController] Metric type deleted successfully: {Id}", id);
            return Ok("Metric type deleted successfully.");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("[MetricsController] DeleteMetric failed - Metric not found: {Id}", id);
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Simplified proxy to Prometheus. Does not require JWT but does require the API key middleware.
    /// Use this endpoint to execute instant or range queries against the configured Prometheus server.
    /// </summary>
    [HttpPost("query")]
    public async Task<IActionResult> QueryPrometheus([FromBody] PrometheusQueryDto dto)
    {
        var currentUser = User.Identity?.Name ?? "unknown";
        _logger.LogInformation("[MetricsController] POST /api/Metrics/query - User: {User}, MetricIds: [{MetricIds}], IsRange: {IsRange}",
            currentUser, dto.MetricIds != null ? string.Join(", ", dto.MetricIds) : "null", dto.IsRange);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[MetricsController] QueryPrometheus validation failed: {Errors}",
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(ModelState);
        }

        try
        {
            string result = await _prometheusService.QueryAsync(dto);
            _logger.LogDebug("[MetricsController] QueryPrometheus succeeded - Response length: {Length} bytes", result.Length);
            return Content(result, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MetricsController] QueryPrometheus failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}