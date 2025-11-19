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

    public async Task<string> QueryAsync(PrometheusQueryDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        MetricType? metricType = await _context.MetricTypes.FindAsync(new object[] { dto.MetricId }, cancellationToken);

        if (metricType == null) throw new KeyNotFoundException($"Metric type with ID {dto.MetricId} not found.");

        StringBuilder builder = new StringBuilder();
        
        string query = dto.Query;
        
        if (!string.IsNullOrWhiteSpace(dto.Instance))
        {
            query = $"{query}{{instance=\"{dto.Instance}\"}}";
        }

        if (dto.IsRange)
        {
            builder.Append("/api/v1/query_range?");
            builder.Append("query=").Append(Uri.EscapeDataString(metricType.PrometheusIdentifier));
            builder.Append("query=").Append(Uri.EscapeDataString(query));

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
            builder.Append("query=").Append(Uri.EscapeDataString(query));

            DateTime time = dto.Time ?? DateTime.UtcNow;
            builder.Append("&time=").Append(((DateTimeOffset)time).ToUnixTimeSeconds());
        }

        string url = builder.ToString();

        using var res = await _http.GetAsync(url);
        string content = await res.Content.ReadAsStringAsync();

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

}
