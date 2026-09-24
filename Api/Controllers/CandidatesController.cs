using System.IO;
using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Feature.Command.Candidates;
using JobApplication.Application.Feature.Query.Candidates;
using JobApplication.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobApplication.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CandidatesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public CandidatesController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var candidates = await _mediator.Send(new GetAllCandidatesQuery(), cancellationToken);
        return Ok(candidates);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var candidateId = _currentUserService.CandidateId;
        if (!candidateId.HasValue)
        {
            return Unauthorized(new { error = "Authenticated candidate profile was not found." });
        }

        try
        {
            var candidate = await _mediator.Send(new GetCandidateByIdQuery(candidateId.Value), cancellationToken);
            return Ok(candidate);
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

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var candidate = await _mediator.Send(new GetCandidateByIdQuery(id), cancellationToken);
            return Ok(candidate);
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

    [HttpPut("me")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateCandidateRequest request, CancellationToken cancellationToken)
    {
        var candidateId = _currentUserService.CandidateId;
        if (!candidateId.HasValue)
        {
            return Unauthorized(new { error = "Authenticated candidate profile was not found." });
        }

        try
        {
            var updated = await _mediator.Send(new UpdateCandidateCommand(candidateId.Value, request.Name, request.CvUrl), cancellationToken);
            return Ok(updated);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCandidateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _mediator.Send(new UpdateCandidateCommand(id, request.Name, request.CvUrl), cancellationToken);
            return Ok(updated);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("cv")]
    [HttpPost("me/cv")]
    [Authorize(Roles = "Candidate")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadCv(IFormFile file, CancellationToken cancellationToken)
    {
        var candidateId = _currentUserService.CandidateId;
        if (!candidateId.HasValue)
        {
            return Unauthorized(new { error = "Authenticated candidate profile was not found." });
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "A valid file must be provided." });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var candidate = await _mediator.Send(
                new UploadCandidateCvCommand(candidateId.Value, stream, file.FileName, file.ContentType, file.Length),
                cancellationToken);

            return Ok(candidate);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:int}/cv")]
    [Authorize(Roles = "Admin,Candidate")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadCvById(int id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "A valid file must be provided." });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var candidate = await _mediator.Send(
                new UploadCandidateCvCommand(id, stream, file.FileName, file.ContentType, file.Length),
                cancellationToken);

            return Ok(candidate);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("cv")]
    [HttpDelete("me/cv")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> DeleteCv(CancellationToken cancellationToken)
    {
        var candidateId = _currentUserService.CandidateId;
        if (!candidateId.HasValue)
        {
            return Unauthorized(new { error = "Authenticated candidate profile was not found." });
        }

        try
        {
            var candidate = await _mediator.Send(new DeleteCandidateCvCommand(candidateId.Value), cancellationToken);
            return Ok(candidate);
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

    [HttpDelete("{id:int}/cv")]
    [Authorize(Roles = "Admin,Candidate")]
    public async Task<IActionResult> DeleteCvById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var candidate = await _mediator.Send(new DeleteCandidateCvCommand(id), cancellationToken);
            return Ok(candidate);
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

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Candidate")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new DeleteCandidateCommand(id), cancellationToken);
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
    }
}
