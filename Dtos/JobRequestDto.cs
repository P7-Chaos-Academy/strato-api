using System.ComponentModel.DataAnnotations;

namespace stratoapi.Dtos;

public class JobRequestDto
{
    [Required] public string prompt { get; set; } 
    [Required] public int n_predict { get; set; } 
    [Required] public float temperature { get; set; } 
    [Required] public int ClusterId { get; set; }
}