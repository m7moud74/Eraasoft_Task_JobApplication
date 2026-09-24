using System.Security.Claims;
using JobApplication.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace JobApplication.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId =>
        User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User?.FindFirst("sub")?.Value;

    public int? CandidateId
    {
        get
        {
            var claim = User?.FindFirst("candidate_id")?.Value
                ?? User?.FindFirst("CandidateId")?.Value;

            if (int.TryParse(claim, out var candidateId))
            {
                return candidateId;
            }

            return null;
        }
    }

    public int? CompanyId
    {
        get
        {
            var claim = User?.FindFirst("company_id")?.Value
                ?? User?.FindFirst("CompanyId")?.Value;

            if (int.TryParse(claim, out var companyId))
            {
                return companyId;
            }

            return null;
        }
    }

    public int? RecruiterId
    {
        get
        {
            var claim = User?.FindFirst("recruiter_id")?.Value
                ?? User?.FindFirst("RecruiterId")?.Value;

            if (int.TryParse(claim, out var recruiterId))
            {
                return recruiterId;
            }

            return null;
        }
    }

    public string? Email =>
        User?.FindFirst(ClaimTypes.Email)?.Value
        ?? User?.FindFirst("email")?.Value;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin => User?.IsInRole("Admin") ?? false;

    public bool IsCandidate => User?.IsInRole("Candidate") ?? false;

    public bool IsRecruiter => User?.IsInRole("Recruiter") ?? false;

    public bool IsCompany => User?.IsInRole("Company") ?? false;

    public bool IsCompanyOwner
    {
        get
        {
            var claim = User?.FindFirst("is_company_owner")?.Value
                ?? User?.FindFirst("IsCompanyOwner")?.Value;

            if (bool.TryParse(claim, out var isOwner))
            {
                return isOwner;
            }

            return IsCompany;
        }
    }
}
