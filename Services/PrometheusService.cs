using System.Text;
using System.Text.Json;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using stratoapi.Data;
using stratoapi.Dtos;
using stratoapi.Models;

namespace stratoapi.Services;

/// <summary>
/// Simple Prometheus HTTP API client used by the application to execute instant and range queries.
/// </summary>
public class PrometheusService : IPrometheusService
{
    private readonly ApplicationDbContext _context;
    private readonly IClusterService _clusterService;

    public PrometheusService(ApplicationDbContext context,  IClusterService clusterService)
    {
        _context = context;
        _clusterService = clusterService;
    }

    public async Task<string> QueryAsync(PrometheusQueryDto dto, int clusterId)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        if (clusterId <= 0) throw new ArgumentOutOfRangeException(nameof(clusterId));

        if (dto.MetricIds == null || dto.MetricIds.Count == 0)
            throw new ArgumentException("At least one MetricId must be provided in MetricIds.");

        string prometheusBaseUrl = await _clusterService.GetClusterPrometheusEndpoint(clusterId);
        
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(prometheusBaseUrl);
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        // Load all requested metric types in one query
        var metricTypes = await _context.MetricTypes
            .Where(mt => dto.MetricIds.Contains(mt.Id))
            .ToListAsync();

        var missing = dto.MetricIds.Except(metricTypes.Select(m => m.Id)).ToList();
        if (missing.Any())
        {
            throw new KeyNotFoundException($"Metric types not found: {string.Join(',', missing)}");
        }

        // Build list of individual identifier queries (one per Prometheus metric name)
        var queries = new List<string>();
        foreach (var mt in metricTypes)
        {
            var identifier = mt.PrometheusIdentifier ?? string.Empty;

            if (identifier.Contains(','))
            {
                var ids = identifier.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
                foreach (var id in ids)
                {
                    var q = id;
                    if (!string.IsNullOrWhiteSpace(dto.Instance)) q = $"{q}{{instance=\"{dto.Instance}\"}}";
                    queries.Add(q);
                }
            }
            else
            {
                var q = identifier.Trim();
                if (!string.IsNullOrWhiteSpace(dto.Instance)) q = $"{q}{{instance=\"{dto.Instance}\"}}";
                queries.Add(q);
            }
        }

        DateTime start = dto.Start ?? DateTime.UtcNow.AddHours(-1);
        DateTime end = dto.End ?? DateTime.UtcNow;
        string step = string.IsNullOrWhiteSpace(dto.Step) ? "15s" : dto.Step;
        DateTime time = dto.Time ?? DateTime.UtcNow;

        // Execute all queries (one HTTP request per query) in parallel
        var tasks = queries.Select(async q =>
        {
            var sb = new StringBuilder();
            if (dto.IsRange)
            {
                sb.Append("api/v1/query_range?query=").Append(Uri.EscapeDataString(q));
                sb.Append("&start=").Append(((DateTimeOffset)start).ToUnixTimeSeconds());
                sb.Append("&end=").Append(((DateTimeOffset)end).ToUnixTimeSeconds());
                sb.Append("&step=").Append(Uri.EscapeDataString(step));
            }
            else
            {
                sb.Append("api/v1/query?query=").Append(Uri.EscapeDataString(q));
                sb.Append("&time=").Append(((DateTimeOffset)time).ToUnixTimeSeconds());
            }

            var res = await httpClient.GetAsync(sb.ToString());
            var content = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Prometheus returned {(int)res.StatusCode}: {content}");
            }
            return content;
        }).ToList();

        var contents = await Task.WhenAll(tasks);

        // Merge results from all responses
        string? resultType = null;
        var mergedResults = new List<string>();

        foreach (var c in contents)
        {
            using var doc = JsonDocument.Parse(c);
            var root = doc.RootElement;
            if (root.TryGetProperty("data", out var dataElem) && dataElem.TryGetProperty("result", out var resultElem))
            {
                if (dataElem.TryGetProperty("resultType", out var rt))
                {
                    var rtStr = rt.GetString();
                    if (resultType == null)
                    {
                        resultType = rtStr;
                    }
                    else if (resultType != rtStr)
                    {
                        throw new InvalidOperationException("Mixed Prometheus result types when merging multiple queries.");
                    }
                }

                foreach (var item in resultElem.EnumerateArray())
                {
                    mergedResults.Add(item.GetRawText());
                }
            }
            else
            {
                throw new JsonException("Unexpected Prometheus response structure when merging results.");
            }
        }

        // Build combined JSON
        using var msOut = new System.IO.MemoryStream();
        using var writerOut = new Utf8JsonWriter(msOut, new JsonWriterOptions { Indented = false });

        writerOut.WriteStartObject();
        writerOut.WriteString("status", "success");
        writerOut.WritePropertyName("data");
        writerOut.WriteStartObject();
        writerOut.WriteString("resultType", resultType ?? (dto.IsRange ? "matrix" : "vector"));
        writerOut.WritePropertyName("result");
        writerOut.WriteStartArray();

        foreach (var elemJson in mergedResults)
        {
            writerOut.WriteRawValue(elemJson);
        }

        writerOut.WriteEndArray();
        writerOut.WriteEndObject();

        // If only a single metric type was requested, and it has a unit, add top-level unit (preserve old behavior)
        if (dto.MetricIds.Count == 1)
        {
            var singleMt = metricTypes.First();
            if (!string.IsNullOrWhiteSpace(singleMt.Unit))
            {
                writerOut.WriteString("unit", singleMt.Unit);
            }
        }

        writerOut.WriteEndObject();
        await writerOut.FlushAsync();

        return Encoding.UTF8.GetString(msOut.ToArray());
    }

}
