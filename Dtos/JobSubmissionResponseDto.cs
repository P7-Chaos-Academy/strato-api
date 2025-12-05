using System.ComponentModel.DataAnnotations;

namespace stratoapi.Dtos;

public class JobSubmissionResponseDto
{
    [Required]
    public JobResponseDto JobDetails { get; set; } = new();
    
    [Required]
    public double EstimatedTimeRemainingSeconds { get; set; }
}

