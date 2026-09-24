namespace JobApplication.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    int? CandidateId { get; }
    int? CompanyId { get; }
    int? RecruiterId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsCandidate { get; }
    bool IsRecruiter { get; }
    bool IsCompany { get; }
    bool IsCompanyOwner { get; }
}
