using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Feature.Query.Applications;

public record GetApplicationByIdQuery(int ApplicationId, int CurrentCandidateId, bool IsAdmin) : IRequest<JobCandidateApplicationDto>;

public class GetApplicationByIdQueryHandler : IRequestHandler<GetApplicationByIdQuery, JobCandidateApplicationDto>
{
    private readonly IJobCandidateApplicationRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public GetApplicationByIdQueryHandler(
        IJobCandidateApplicationRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<JobCandidateApplicationDto> Handle(GetApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        var application = await _repository.GetDetailsByIdAsync(request.ApplicationId, cancellationToken);
        if (application is null)
        {
            throw new NotFoundException($"Job application with ID {request.ApplicationId} was not found.");
        }

        var isJobOwner = application.Job is not null &&
                         !string.IsNullOrWhiteSpace(application.Job.CreatedByUserId) &&
                         application.Job.CreatedByUserId == _currentUserService.UserId;

        if (!request.IsAdmin && !isJobOwner && application.CandidateId != request.CurrentCandidateId)
        {
            throw new ForbiddenAccessException("You are not authorized to view this job application.");
        }

        return new JobCandidateApplicationDto
        {
            Id = application.Id,
            CandidateId = application.CandidateId,
            CandidateName = application.Candidate?.Name ?? string.Empty,
            CandidateEmail = application.Candidate?.Email ?? string.Empty,
            JobId = application.JobId,
            JobTitle = application.Job?.Title ?? string.Empty,
            Status = application.JobApplicationStatus,
            AppliedAt = application.AppliedAt,
            StatusUpdatedAt = application.StatusUpdatedAt,
            CancelledAt = application.CancelledAt
        };
    }
}
