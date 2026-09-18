namespace JobApplication.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    int? CandidateId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsCandidate { get; }
    bool IsRecruiter { get; }
}
