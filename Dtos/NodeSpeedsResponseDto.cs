using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace stratoapi.Dtos;

public class NodeSpeedsResponseDto
{
    [Required]
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [Required]
    [JsonPropertyName("node_speeds")]
    public Dictionary<string, double> NodeSpeeds { get; set; } = new();
}

