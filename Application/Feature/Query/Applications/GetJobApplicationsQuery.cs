using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Query.Applications;

public record GetJobApplicationsQuery(int JobId) : IRequest<IReadOnlyList<JobCandidateApplicationDto>>;

public class GetJobApplicationsQueryHandler : IRequestHandler<GetJobApplicationsQuery, IReadOnlyList<JobCandidateApplicationDto>>
{
    private readonly IJobCandidateApplicationRepository _repository;
    private readonly IJobRepository _jobRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetJobApplicationsQueryHandler(
        IJobCandidateApplicationRepository repository,
        IJobRepository jobRepository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _jobRepository = jobRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<JobCandidateApplicationDto>> Handle(GetJobApplicationsQuery request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null)
        {
            throw new NotFoundException($"Job with ID {request.JobId} was not found.");
        }

        if (!_currentUserService.IsAdmin &&
            !string.IsNullOrWhiteSpace(job.CreatedByUserId) &&
            job.CreatedByUserId != _currentUserService.UserId)
        {
            throw new ForbiddenAccessException("Only the recruiter who opened this job or an administrator can view its applications.");
        }

        var applications = await _repository.GetByJobIdAsync(request.JobId, cancellationToken);
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
