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
    private readonly ILogger<JobController> _logger;

    public JobController(IJobService jobService, ILogger<JobController> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> PostJob([FromBody] JobRequestDto jobRequestDto)
    {
        var currentUser = User.Identity?.Name ?? "unknown";
        _logger.LogInformation("[JobController] POST /api/Job - User: {User}, TokenCount: {TokenCount}, Temperature: {Temperature}",
            currentUser, jobRequestDto.n_predict, jobRequestDto.temperature);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[JobController] PostJob validation failed: {Errors}",
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(ModelState);
        }

        IActionResult jobResponse = await _jobService.PostJob(jobRequestDto);
        IActionResult estimatedJobTimeResult = await _jobService.GetEstimatedTimeRemaining(jobRequestDto.n_predict);

        // Check if job submission was successful
        if (jobResponse is not OkObjectResult jobOkResult || jobOkResult.Value is not JobResponseDto jobData)
        {
            _logger.LogWarning("[JobController] PostJob failed - returning error response");
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

        _logger.LogInformation("[JobController] PostJob succeeded - JobId: {JobId}, EstimatedTime: {EstimatedTime}s",
            jobData.JobId, estimatedTimeSeconds);

        return Ok(combinedResponse);
    }

    [HttpGet("jobId/{jobId}")]
    public async Task<IActionResult> GetJobStatus([FromRoute] string jobId)
    {
        _logger.LogInformation("[JobController] GET /api/Job/jobId/{JobId}", jobId);

        if (string.IsNullOrWhiteSpace(jobId))
        {
            _logger.LogWarning("[JobController] GetJobStatus - Job ID is null or empty");
            return BadRequest("Job ID cannot be null or empty.");
        }

        IActionResult jobStatusResponse = await _jobService.GetJobStatus(jobId);
        return jobStatusResponse;
    }

    [HttpGet("all-jobs")]
    public async Task<IActionResult> GetAllJobs()
    {
        var currentUser = User.Identity?.Name ?? "unknown";
        _logger.LogInformation("[JobController] GET /api/Job/all-jobs - User: {User}", currentUser);

        IActionResult allJobsResponse = await _jobService.GetAllJobs();
        return allJobsResponse;
    }
}