using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Feature.Command.Companies;
using JobApplication.Application.Feature.Query.Companies;
using JobApplication.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobApplication.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompaniesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Register([FromBody] RegisterCompanyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var company = await _mediator.Send(new RegisterCompanyCommand(request), cancellationToken);
            return StatusCode(StatusCodes.Status201Created, new
            {
                message = "Company registration request submitted successfully. It is pending administrator approval.",
                company
            });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] CompanyStatus? status, CancellationToken cancellationToken)
    {
        try
        {
            var companies = await _mediator.Send(new GetAllCompaniesQuery(status), cancellationToken);
            return Ok(companies);
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        try
        {
            var pendingCompanies = await _mediator.Send(new GetPendingCompaniesQuery(), cancellationToken);
            return Ok(pendingCompanies);
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
            var company = await _mediator.Send(new GetCompanyByIdQuery(id), cancellationToken);
            return Ok(company);
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

    [HttpPut("{id:int}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        try
        {
            var company = await _mediator.Send(new ApproveCompanyCommand(id), cancellationToken);
            return Ok(new
            {
                message = "Company approved successfully.",
                company
            });
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

    [HttpPut("{id:int}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(int id, CancellationToken cancellationToken)
    {
        try
        {
            var company = await _mediator.Send(new RejectCompanyCommand(id), cancellationToken);
            return Ok(new
            {
                message = "Company registration rejected.",
                company
            });
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

    [HttpPost("recruiters")]
    [Authorize(Roles = "Company,Recruiter,Admin")]
    public async Task<IActionResult> AddRecruiter([FromBody] AddRecruiterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var recruiter = await _mediator.Send(new AddRecruiterCommand(request), cancellationToken);
            return StatusCode(StatusCodes.Status201Created, recruiter);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { error = ex.Message });
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

    [HttpPost("{companyId:int}/recruiters")]
    [Authorize(Roles = "Company,Recruiter,Admin")]
    public async Task<IActionResult> AddRecruiterToCompany(int companyId, [FromBody] AddRecruiterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            request.CompanyId = companyId;
            var recruiter = await _mediator.Send(new AddRecruiterCommand(request), cancellationToken);
            return StatusCode(StatusCodes.Status201Created, recruiter);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { error = ex.Message });
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

    [HttpGet("recruiters")]
    [Authorize(Roles = "Company,Recruiter,Admin")]
    public async Task<IActionResult> GetMyRecruiters(CancellationToken cancellationToken)
    {
        try
        {
            var recruiters = await _mediator.Send(new GetCompanyRecruitersQuery(), cancellationToken);
            return Ok(recruiters);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("{companyId:int}/recruiters")]
    [Authorize(Roles = "Company,Recruiter,Admin")]
    public async Task<IActionResult> GetCompanyRecruiters(int companyId, CancellationToken cancellationToken)
    {
        try
        {
            var recruiters = await _mediator.Send(new GetCompanyRecruitersQuery(companyId), cancellationToken);
            return Ok(recruiters);
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
}
