using System.Text;
using System.Text.Json;
using stratoapi.Data;
using stratoapi.Dtos;
using stratoapi.Models;

namespace stratoapi.Services;

/// <summary>
/// Simple Prometheus HTTP API client used by the application to execute instant and range queries.
/// </summary>
public class PrometheusService : IPrometheusService
{
    private readonly HttpClient _http;
    private readonly ApplicationDbContext _context;

    public PrometheusService(HttpClient http, ApplicationDbContext context)
    {
        _http = http;
        _context = context;
    }

    public async Task<string> QueryAsync(PrometheusQueryDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        MetricType? metricType = await _context.MetricTypes.FindAsync(new object[] { dto.MetricId }, cancellationToken);

        if (metricType == null) throw new KeyNotFoundException($"Metric type with ID {dto.MetricId} not found.");

        var builder = new StringBuilder();
        if (dto.IsRange)
        {
            builder.Append("/api/v1/query_range?");
            builder.Append("query=").Append(Uri.EscapeDataString(metricType.PrometheusIdentifier));

            DateTime start = dto.Start ?? DateTime.UtcNow.AddHours(-1);
            DateTime end = dto.End ?? DateTime.UtcNow;
            string? step = string.IsNullOrWhiteSpace(dto.Step) ? "15s" : dto.Step;

            builder.Append("&start=").Append(((DateTimeOffset)start).ToUnixTimeSeconds());
            builder.Append("&end=").Append(((DateTimeOffset)end).ToUnixTimeSeconds());
            builder.Append("&step=").Append(Uri.EscapeDataString(step));
        }
        else
        {
            builder.Append("/api/v1/query?");
            builder.Append("query=").Append(Uri.EscapeDataString(metricType.PrometheusIdentifier));

            DateTime time = dto.Time ?? DateTime.UtcNow;
            builder.Append("&time=").Append(((DateTimeOffset)time).ToUnixTimeSeconds());
        }

        string url = builder.ToString();

        using var res = await _http.GetAsync(url, cancellationToken);
        string content = await res.Content.ReadAsStringAsync(cancellationToken);

        // Return raw JSON from Prometheus; throw on severe errors so callers can surface status codes.
        if (!res.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Prometheus returned {(int)res.StatusCode}: {content}");
        }

        // If unit is defined, parse the JSON and add unit as a property in the response
        if (!string.IsNullOrWhiteSpace(metricType.Unit))
        {
            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                
                // Parse and reconstruct JSON with unit property
                using var ms = new System.IO.MemoryStream();
                using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = false });
                
                writer.WriteStartObject();
                
                // Copy existing properties
                foreach (var property in root.EnumerateObject())
                {
                    writer.WritePropertyName(property.Name);
                    property.Value.WriteTo(writer);
                }
                
                // Add unit property
                writer.WriteString("unit", metricType.Unit);
                
                writer.WriteEndObject();
                await writer.FlushAsync();
                
                content = Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (JsonException)
            {
                // If parsing fails, return content as-is with unit comment for debugging
                content += $"\n/* Unit: {metricType.Unit} */";
            }
        }

        return content;
    }

    /// <summary>
    /// Retrieves metric names from Prometheus using the label values endpoint for __name__.
    /// </summary>
    public async Task<List<string>> GetMetricNamesAsync(CancellationToken cancellationToken = default)
    {
        using var res = await _http.GetAsync("/api/v1/label/__name__/values", cancellationToken);
        var content = await res.Content.ReadAsStringAsync(cancellationToken);

        if (!res.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Prometheus returned {(int)res.StatusCode}: {content}");
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                var list = new List<string>();
                foreach (var el in data.EnumerateArray())
                {
                    if (el.ValueKind == JsonValueKind.String)
                        list.Add(el.GetString()!);
                }

                return list;
            }
        }
        catch (JsonException)
        {
            // fallthrough to return empty list after logging by caller
        }

        return new List<string>();
    }
}
