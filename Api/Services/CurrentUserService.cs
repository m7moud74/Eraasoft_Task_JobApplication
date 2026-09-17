using System.Security.Claims;
using JobApplication.Application.Interfaces;

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

    public string? Email =>
        User?.FindFirst(ClaimTypes.Email)?.Value
        ?? User?.FindFirst("email")?.Value;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin => User?.IsInRole("Admin") ?? false;

    public bool IsCandidate => User?.IsInRole("Candidate") ?? false;
}
