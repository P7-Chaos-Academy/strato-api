using System.Text.Json.Serialization;

namespace stratoapi.Dtos;

public class JobStatusResponseDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("job_name")]
    public string JobName { get; set; } = string.Empty;

    [JsonPropertyName("namespace")]
    public string Namespace { get; set; } = string.Empty;

    [JsonPropertyName("pod_name")]
    public string PodName { get; set; } = string.Empty;

    [JsonPropertyName("node_name")]
    public string NodeName { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    public string Result { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("started_at")]
    public string? StartedAt { get; set; }

    [JsonPropertyName("completed_at")]
    public string? CompletedAt { get; set; }

    [JsonPropertyName("duration_seconds")]
    public double? DurationSeconds { get; set; }

    [JsonPropertyName("power_consumed_wh")]
    public double? PowerConsumedWh { get; set; }

    [JsonPropertyName("token_count")]
    public int? TokenCount { get; set; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }
    
}