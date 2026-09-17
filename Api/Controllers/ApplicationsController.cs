using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobApplication.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly IJobApplicationService _applicationService;

    public ApplicationsController(IJobApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
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

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var candidateId))
        {
            return Unauthorized(new { error = "User identity is required. Provide authenticated claims or an X-Candidate-Id header." });
        }

        try
        {
            await _applicationService.CancelAsync(id, candidateId, cancellationToken);
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
