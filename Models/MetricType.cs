using System.ComponentModel.DataAnnotations;

namespace stratoapi.Models;

public class MetricType : BaseModel
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public string PrometheusIdentifier { get; set; }
}