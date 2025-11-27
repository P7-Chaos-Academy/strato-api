using System.ComponentModel.DataAnnotations;

namespace stratoapi.Models;

public class MetricType : BaseModel
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public List<string> PrometheusIdentifier { get; set; } = new List<string>();

    public string? Unit { get; set; }
}