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
    private readonly IHangFrieService _hangfireService;
    private readonly IAuditService _auditService;

    public UpdateApplicationStatusCommandHandler(
        IJobCandidateApplicationRepository repository,
        ICurrentUserService currentUserService,
        IHangFrieService hangfireService,
        IAuditService auditService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _hangfireService = hangfireService;
        _auditService = auditService;
    }

    public async Task<JobCandidateApplicationDto> Handle(UpdateApplicationStatusCommand request, CancellationToken cancellationToken)
    {
        var application = await _repository.GetDetailsByIdAsync(request.ApplicationId, cancellationToken);
        if (application is null)
        {
            throw new NotFoundException($"Job application with ID {request.ApplicationId} was not found.");
        }

        if (!_currentUserService.IsAdmin)
        {
            var userCompanyId = _currentUserService.CompanyId;
            if (application.Job is not null && (!userCompanyId.HasValue || userCompanyId.Value != application.Job.CompanyId))
            {
                throw new ForbiddenAccessException("You cannot change the status of an application belonging to another company's job.");
            }

            if (application.Job is not null &&
                !string.IsNullOrWhiteSpace(application.Job.CreatedByUserId) &&
                application.Job.CreatedByUserId != _currentUserService.UserId &&
                !_currentUserService.IsCompanyOwner)
            {
                throw new ForbiddenAccessException("Only the recruiter who opened this job can change the status of its applications.");
            }
        }

        var oldStatus = application.JobApplicationStatus;
        var newStatus = request.Status;

        application.UpdateStatus(newStatus);

        _repository.Update(application);
        await _repository.SaveChangesAsync(cancellationToken);

        // Record audit log
        if (oldStatus != newStatus)
        {
            await _auditService.LogAsync("Application status changed", "JobCandidateApplication", application.Id.ToString(), $"Status changed from {oldStatus} to {newStatus}.", cancellationToken);
        }

        // Enqueue background email only after successful save and when meaningful status change occurs
        if (oldStatus != newStatus &&
            (newStatus == JobApplicationStatus.UnderReview ||
             newStatus == JobApplicationStatus.InterView ||
             newStatus == JobApplicationStatus.Accepted ||
             newStatus == JobApplicationStatus.Rejected))
        {
            _hangfireService.Enqueue<IEmailNotificationJob>(job =>
                job.SendApplicationStatusChangedNotificationAsync(request.ApplicationId, oldStatus, newStatus));
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
