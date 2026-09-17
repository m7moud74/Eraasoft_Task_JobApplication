using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobApplication.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly IJobApplicationService _applicationService;
    private readonly ICurrentUserService _currentUserService;

    public ApplicationsController(IJobApplicationService applicationService, ICurrentUserService currentUserService)
    {
        _applicationService = applicationService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> Apply([FromBody] ApplyJobRequest request, CancellationToken cancellationToken)
    {
        var candidateId = _currentUserService.CandidateId;
        if (!candidateId.HasValue)
        {
            return Unauthorized(new { error = "Authenticated candidate profile was not found." });
        }

        try
        {
            var application = await _applicationService.ApplyAsync(request.JobId, candidateId.Value, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = application.Id }, application);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var application = await _applicationService.GetByIdAsync(
                id,
                _currentUserService.CandidateId ?? 0,
                _currentUserService.IsAdmin,
                cancellationToken);

            return Ok(application);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }

    [HttpGet("my")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetMyApplications(CancellationToken cancellationToken)
    {
        var candidateId = _currentUserService.CandidateId;
        if (!candidateId.HasValue)
        {
            return Unauthorized(new { error = "Authenticated candidate profile was not found." });
        }

        var applications = await _applicationService.GetMyApplicationsAsync(candidateId.Value, cancellationToken);
        return Ok(applications);
    }

    [HttpGet("job/{jobId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetJobApplications(int jobId, CancellationToken cancellationToken)
    {
        try
        {
            var applications = await _applicationService.GetJobApplicationsAsync(jobId, cancellationToken);
            return Ok(applications);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateApplicationStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _applicationService.UpdateStatusAsync(id, request.Status, cancellationToken);
            return Ok(updated);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        // 1. Check authenticated identity
        int? candidateId = _currentUserService.CandidateId;

        // 2. Legacy fallback for claims / headers only if not already resolved via authenticated identity
        if (!candidateId.HasValue)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("candidate_id")?.Value
                ?? User.FindFirst("CandidateId")?.Value
                ?? User.FindFirst("id")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                if (Request.Headers.TryGetValue("X-Candidate-Id", out var headerVal) ||
                    Request.Headers.TryGetValue("X-User-Id", out headerVal))
                {
                    userIdClaim = headerVal.ToString();
                }
            }

            if (int.TryParse(userIdClaim, out var parsedId))
            {
                candidateId = parsedId;
            }
        }

        if (!candidateId.HasValue)
        {
            return Unauthorized(new { error = "User identity is required. Provide authenticated claims or an X-Candidate-Id header." });
        }

        try
        {
            await _applicationService.CancelAsync(id, candidateId.Value, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }
}
