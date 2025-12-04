using Microsoft.AspNetCore.Mvc;
using stratoapi.Dtos;

namespace stratoapi.Services;

public interface IJobService
{
    public Task<IActionResult> PostJob(JobRequestDto dto);

    public Task<IActionResult> GetEstimatedTimeRemaining(int tokenCount);
    
    public Task<IActionResult> GetJobStatus(string jobId);
}