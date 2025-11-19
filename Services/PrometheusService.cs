using System.Text;
using System.Text.Json;
using stratoapi.Dtos;

namespace stratoapi.Services;

/// <summary>
/// Simple Prometheus HTTP API client used by the application to execute instant and range queries.
/// </summary>
public class PrometheusService : IPrometheusService
{
    private readonly HttpClient _http;

    public PrometheusService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> QueryAsync(PrometheusQueryDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        StringBuilder builder = new StringBuilder();
        
        string query = dto.Query;
        
        if (!string.IsNullOrWhiteSpace(dto.Instance))
        {
            query = $"{query}{{instance=\"{dto.Instance}\"}}";
        }

        if (dto.IsRange)
        {
            builder.Append("/api/v1/query_range?");
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

        return content;
    }

}
