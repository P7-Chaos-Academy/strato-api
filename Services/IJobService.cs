using Microsoft.AspNetCore.Mvc;
using stratoapi.Dtos;

namespace stratoapi.Services;

public interface IJobService
{
    public Task<IActionResult> PostJob(JobRequestDto dto);

    public Task<IActionResult> GetEstimatedTimeRemaining(int tokenCount, int clusterId);
    
    public Task<IActionResult> GetJobStatus(string jobId, int clusterId);
    public Task<IActionResult> GetAllJobs(int clusterId);
}