using System.ComponentModel.DataAnnotations;

namespace stratoapi.Dtos;

/// <summary>
/// Request DTO for simplified Prometheus queries.
/// - For instant queries set <see cref="IsRange"/> = false and optionally set <see cref="Time"/>.
/// - For range queries set <see cref="IsRange"/> = true and provide <see cref="Start"/>, <see cref="End"/> and <see cref="Step"/>.
/// </summary>
public class PrometheusQueryDto
{
    [Required]
    public required int MetricId { get; set; }

    /// <summary>
    /// When performing an instant query this optional value sets the evaluation time (UTC). If null now is used.
    /// </summary>
    public DateTime? Time { get; set; }

    /// <summary>
    /// When performing a range query this is the start time (UTC).
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    /// When performing a range query this is the end time (UTC).
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    /// The step duration for range queries (e.g. "15s", "1m").
    /// </summary>
    public string? Step { get; set; }

    /// <summary>
    /// True to run a range query (/api/v1/query_range) otherwise an instant query (/api/v1/query)
    /// </summary>
    public bool IsRange { get; set; } = false;
}
