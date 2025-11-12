using System.ComponentModel.DataAnnotations;

namespace stratoapi.Dtos;

public class MetricsDto
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public string PrometheusIdentifier { get; set; }
}