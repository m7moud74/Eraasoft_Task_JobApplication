using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Query.Applications;

public record GetMyApplicationsQuery(int CandidateId) : IRequest<IReadOnlyList<JobCandidateApplicationDto>>;

public class GetMyApplicationsQueryHandler : IRequestHandler<GetMyApplicationsQuery, IReadOnlyList<JobCandidateApplicationDto>>
{
    private readonly IJobCandidateApplicationRepository _repository;

    public GetMyApplicationsQueryHandler(IJobCandidateApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<JobCandidateApplicationDto>> Handle(GetMyApplicationsQuery request, CancellationToken cancellationToken)
    {
        var applications = await _repository.GetByCandidateIdAsync(request.CandidateId, cancellationToken);
        return applications.Select(app => new JobCandidateApplicationDto
        {
            Id = app.Id,
            CandidateId = app.CandidateId,
            CandidateName = app.Candidate?.Name ?? string.Empty,
            CandidateEmail = app.Candidate?.Email ?? string.Empty,
            JobId = app.JobId,
            JobTitle = app.Job?.Title ?? string.Empty,
            Status = app.JobApplicationStatus,
            AppliedAt = app.AppliedAt,
            StatusUpdatedAt = app.StatusUpdatedAt,
            CancelledAt = app.CancelledAt
        }).ToList();
    }
}
