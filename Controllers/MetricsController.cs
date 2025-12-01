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
    
    /// <summary>
    /// Simplified proxy to Prometheus. Does not require JWT but does require the API key middleware.
    /// Use this endpoint to execute instant or range queries against the configured Prometheus server.
    /// </summary>
    [HttpPost("query")]
    public async Task<IActionResult> QueryPrometheus([FromBody] System.Text.Json.JsonElement body)
    {
        try
        {
            var dto = new PrometheusQueryDto();

            // metricIds can be sent as an array of numbers or as a comma-separated string
            if (body.TryGetProperty("metricIds", out var metricIdsProp))
            {
                if (metricIdsProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    dto.MetricIds = metricIdsProp.EnumerateArray()
                        .Where(x => x.ValueKind == System.Text.Json.JsonValueKind.Number)
                        .Select(x => x.GetInt32())
                        .ToList();
                }
                else if (metricIdsProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var raw = metricIdsProp.GetString();
                    dto.MetricIds = raw?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => int.TryParse(s.Trim(), out var v) ? v : (int?)null)
                        .Where(v => v.HasValue)
                        .Select(v => v!.Value)
                        .ToList();
                }
            }

            // Backwards-compatible single id
            if ((dto.MetricIds == null || dto.MetricIds.Count == 0) && body.TryGetProperty("metricId", out var metricIdProp))
            {
                if (metricIdProp.ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    dto.MetricIds = new List<int> { metricIdProp.GetInt32() };
                }
                else if (metricIdProp.ValueKind == System.Text.Json.JsonValueKind.String && int.TryParse(metricIdProp.GetString(), out var v))
                {
                    dto.MetricIds = new List<int> { v };
                }
            }

            // time / startTime / endTime mapping
            if (body.TryGetProperty("time", out var timeProp) && timeProp.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                if (DateTime.TryParse(timeProp.GetString(), out var t)) dto.Time = t;
            }

            if (body.TryGetProperty("startTime", out var startProp) && startProp.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                if (DateTime.TryParse(startProp.GetString(), out var s)) dto.Start = s;
            }

            if (body.TryGetProperty("endTime", out var endProp) && endProp.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                if (DateTime.TryParse(endProp.GetString(), out var e)) dto.End = e;
            }

            // fallback names
            if (dto.Start == null && body.TryGetProperty("start", out var startProp2) && startProp2.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                if (DateTime.TryParse(startProp2.GetString(), out var s2)) dto.Start = s2;
            }

            if (dto.End == null && body.TryGetProperty("end", out var endProp2) && endProp2.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                if (DateTime.TryParse(endProp2.GetString(), out var e2)) dto.End = e2;
            }

            if (body.TryGetProperty("step", out var stepProp) && stepProp.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                dto.Step = stepProp.GetString();
            }

            if (body.TryGetProperty("isRange", out var isRangeProp) && isRangeProp.ValueKind == System.Text.Json.JsonValueKind.True || isRangeProp.ValueKind == System.Text.Json.JsonValueKind.False)
            {
                dto.IsRange = isRangeProp.GetBoolean();
            }

            if (body.TryGetProperty("instance", out var instanceProp) && instanceProp.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                dto.Instance = instanceProp.GetString();
            }

            if (dto.MetricIds == null || dto.MetricIds.Count == 0)
            {
                return BadRequest(new { error = "metricIds or metricId must be provided" });
            }

            string result = await _prometheusService.QueryAsync(dto);
            return Content(result, "application/json");
        }
        catch (System.Text.Json.JsonException je)
        {
            return BadRequest(new { error = "Invalid JSON payload", details = je.Message });
        }
    }
}