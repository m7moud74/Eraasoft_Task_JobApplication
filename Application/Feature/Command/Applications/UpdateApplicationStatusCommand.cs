using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;

namespace JobApplication.Application.Feature.Command.Applications;

public record UpdateApplicationStatusCommand(int ApplicationId, JobApplicationStatus Status) : IRequest<JobCandidateApplicationDto>;

public class UpdateApplicationStatusCommandHandler : IRequestHandler<UpdateApplicationStatusCommand, JobCandidateApplicationDto>
{
    private readonly IJobCandidateApplicationRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateApplicationStatusCommandHandler(
        IJobCandidateApplicationRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<JobCandidateApplicationDto> Handle(UpdateApplicationStatusCommand request, CancellationToken cancellationToken)
    {
        var application = await _repository.GetDetailsByIdAsync(request.ApplicationId, cancellationToken);
        if (application is null)
        {
            throw new NotFoundException($"Job application with ID {request.ApplicationId} was not found.");
        }

        if (application.Job is not null &&
            !string.IsNullOrWhiteSpace(application.Job.CreatedByUserId) &&
            application.Job.CreatedByUserId != _currentUserService.UserId)
        {
            throw new ForbiddenAccessException("Only the recruiter who opened this job can change the status of its applications.");
        }

        application.UpdateStatus(request.Status);

        _repository.Update(application);
        await _repository.SaveChangesAsync(cancellationToken);

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
