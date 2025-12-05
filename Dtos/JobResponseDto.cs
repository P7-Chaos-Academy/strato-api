using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace stratoapi.Dtos;

public class JobResponseDto
{
    [Required] 
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [Required] 
    [JsonPropertyName("job_name")]
    public string JobName { get; set; } = string.Empty;
    
    [Required] 
    [JsonPropertyName("namespace")]
    public string Namespace { get; set; } = string.Empty;
    
    [Required] 
    [JsonPropertyName("uid")]
    public string Uid { get; set; } = string.Empty;
    
    [Required] 
    [JsonPropertyName("creation_timestamp")]
    public string CreationTimestamp { get; set; } = string.Empty;
}