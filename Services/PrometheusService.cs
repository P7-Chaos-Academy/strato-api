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
        MetricType? metricType = await _context.MetricTypes.FindAsync(new object[] { dto.MetricId });

        if (metricType == null) throw new KeyNotFoundException($"Metric type with ID {dto.MetricId} not found.");

        // Work with a list of identifiers
        var identifiers = metricType.PrometheusIdentifier ?? new List<string>();
        var idsList = identifiers
            .Select(s => s?.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        // If multiple identifiers are present, run a separate Prometheus request for each and merge results
        if (idsList.Count > 1)
        {
            DateTime start = dto.Start ?? DateTime.UtcNow.AddHours(-1);
            DateTime end = dto.End ?? DateTime.UtcNow;
            string step = string.IsNullOrWhiteSpace(dto.Step) ? "15s" : dto.Step;
            DateTime time = dto.Time ?? DateTime.UtcNow;

            // Build request tasks
            var tasks = idsList.Select(async idName =>
            {
                var sbLocal = new StringBuilder();
                string q = idName;
                if (!string.IsNullOrWhiteSpace(dto.Instance))
                {
                    q = $"{q}{{instance=\"{dto.Instance}\"}}";
                }

                if (dto.IsRange)
                {
                    sbLocal.Append("/api/v1/query_range?query=").Append(Uri.EscapeDataString(q));
                    sbLocal.Append("&start=").Append(((DateTimeOffset)start).ToUnixTimeSeconds());
                    sbLocal.Append("&end=").Append(((DateTimeOffset)end).ToUnixTimeSeconds());
                    sbLocal.Append("&step=").Append(Uri.EscapeDataString(step));
                }
                else
                {
                    sbLocal.Append("/api/v1/query?query=").Append(Uri.EscapeDataString(q));
                    sbLocal.Append("&time=").Append(((DateTimeOffset)time).ToUnixTimeSeconds());
                }

                var resLocal = await _http.GetAsync(sbLocal.ToString());
                var contentLocal = await resLocal.Content.ReadAsStringAsync();

                if (!resLocal.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Prometheus returned {(int)resLocal.StatusCode}: {contentLocal}");
                }

                return contentLocal;
            }).ToList();

            var contents = await Task.WhenAll(tasks);

            // Merge results
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

                    // Copy each result item's raw JSON text so it remains valid after the JsonDocument is disposed
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
            using var ms = new System.IO.MemoryStream();
            using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = false });

            writer.WriteStartObject();
            writer.WriteString("status", "success");
            writer.WritePropertyName("data");
            writer.WriteStartObject();
            writer.WriteString("resultType", resultType ?? (dto.IsRange ? "matrix" : "vector"));
            writer.WritePropertyName("result");
            writer.WriteStartArray();

            foreach (var elemJson in mergedResults)
            {
                // Write raw JSON value (already valid JSON) into the array
                writer.WriteRawValue(elemJson);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();

            // Add unit at top level if defined
            if (!string.IsNullOrWhiteSpace(metricType.Unit))
            {
                writer.WriteString("unit", metricType.Unit);
            }

            writer.WriteEndObject();
            await writer.FlushAsync();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        // Single-identifier request
        if (idsList.Count == 0)
        {
            throw new InvalidOperationException("No Prometheus identifier configured for this metric type.");
        }

        var singleId = idsList[0];
        string singleQuery = singleId ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(dto.Instance))
        {
            singleQuery = $"{singleQuery}{{instance=\"{dto.Instance}\"}}";
        }

        var builder = new StringBuilder();
        if (dto.IsRange)
        {
            builder.Append("/api/v1/query_range?");
            builder.Append("query=").Append(Uri.EscapeDataString(singleQuery));

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
            builder.Append("query=").Append(Uri.EscapeDataString(singleQuery));

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
