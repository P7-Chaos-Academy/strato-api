using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using stratoapi.Dtos;
using stratoapi.Models;
using stratoapi.Services;

namespace stratoapi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{nameof(AuthRole.SeedUser)},{nameof(AuthRole.Admin)}")]
public class JobController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpPost]
    public async Task<IActionResult> PostJob([FromBody] JobRequestDto jobRequestDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        IActionResult jobResponse = await _jobService.PostJob(jobRequestDto);
        IActionResult estimatedJobTimeResult = await _jobService.GetEstimatedTimeRemaining(jobRequestDto.n_predict, jobRequestDto.ClusterId);
        
        // Check if job submission was successful
        if (jobResponse is not OkObjectResult jobOkResult || jobOkResult.Value is not JobResponseDto jobData)
        {
            return jobResponse; // Return the error response
        }
        
        // Extract estimated time from the result
        double estimatedTimeSeconds = 0;
        if (estimatedJobTimeResult is OkObjectResult estimatedTimeOkResult && estimatedTimeOkResult.Value is not null)
        {
            var estimatedTimeObj = estimatedTimeOkResult.Value;
            var estimatedTimeProp = estimatedTimeObj.GetType().GetProperty("estimatedTimeRemainingSeconds");
            if (estimatedTimeProp != null)
            {
                object? value = estimatedTimeProp.GetValue(estimatedTimeObj);
                if (value != null)
                {
                    estimatedTimeSeconds = Convert.ToDouble(value);
                }
            }
        }
        
        // Return combined response
        JobSubmissionResponseDto combinedResponse = new JobSubmissionResponseDto
        {
            JobDetails = jobData,
            EstimatedTimeRemainingSeconds = estimatedTimeSeconds
        };
        return Ok(combinedResponse);
    }
    
    [HttpGet("jobId/{jobId}")]
    public async Task<IActionResult> GetJobStatus([FromRoute] string jobId, [FromQuery] int clusterId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            return BadRequest("Job ID cannot be null or empty.");
        }
        IActionResult jobStatusResponse = await _jobService.GetJobStatus(jobId, clusterId);
        return jobStatusResponse;
    }

    [HttpGet("all-jobs")]
    public async Task<IActionResult> GetAllJobs([FromQuery] int clusterId)
    {
        IActionResult allJobsResponse = await _jobService.GetAllJobs(clusterId);
        return allJobsResponse;
    }
}