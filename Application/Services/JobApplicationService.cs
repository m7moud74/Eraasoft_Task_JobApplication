using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Services;

public class JobApplicationService : IJobApplicationService
{
    private readonly IJobCandidateApplicationRepository _repository;
    private readonly IJobRepository _jobRepository;
    private readonly ICandidateRepository _candidateRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHangFrieService _hangfireService;
    private readonly IAuditService _auditService;

    public JobApplicationService(
        IJobCandidateApplicationRepository repository,
        IJobRepository jobRepository,
        ICandidateRepository candidateRepository,
        ICurrentUserService currentUserService,
        IHangFrieService hangfireService,
        IAuditService auditService)
    {
        _repository = repository;
        _jobRepository = jobRepository;
        _candidateRepository = candidateRepository;
        _currentUserService = currentUserService;
        _hangfireService = hangfireService;
        _auditService = auditService;
    }

    public async Task<JobCandidateApplicationDto> ApplyAsync(int jobId, int candidateId, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job is null)
        {
            throw new NotFoundException($"Job with ID {jobId} was not found.");
        }

        if (!job.IsActive)
        {
            throw new BadRequestException("Cannot apply to an inactive job.");
        }

        var candidate = await _candidateRepository.GetByIdAsync(candidateId, cancellationToken);
        if (candidate is null)
        {
            throw new NotFoundException($"Candidate with ID {candidateId} was not found.");
        }

        var alreadyApplied = await _repository.ExistsAsync(jobId, candidateId, cancellationToken);
        if (alreadyApplied)
        {
            throw new BadRequestException("You have already applied for this job.");
        }

        var application = new JobCandidateApplication
        {
            JobId = jobId,
            CandidateId = candidateId,
            JobApplicationStatus = JobApplicationStatus.Applied,
            AppliedAt = DateTime.UtcNow,
            StatusUpdatedAt = DateTime.UtcNow
        };

        await _repository.InsertAsync(application, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Application submitted", "JobCandidateApplication", application.Id.ToString(), $"Candidate {candidate.Id} applied to Job {job.Id}.", cancellationToken);

        return new JobCandidateApplicationDto
        {
            Id = application.Id,
            CandidateId = candidate.Id,
            CandidateName = candidate.Name,
            CandidateEmail = candidate.Email,
            JobId = job.Id,
            JobTitle = job.Title,
            Status = application.JobApplicationStatus,
            AppliedAt = application.AppliedAt,
            StatusUpdatedAt = application.StatusUpdatedAt,
            CancelledAt = application.CancelledAt
        };
    }

    public async Task<JobCandidateApplicationDto> GetByIdAsync(int applicationId, int currentCandidateId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var application = await _repository.GetDetailsByIdAsync(applicationId, cancellationToken);
        if (application is null)
        {
            throw new NotFoundException($"Job application with ID {applicationId} was not found.");
        }

        var isJobOwnerCompany = application.Job is not null &&
                                _currentUserService.CompanyId.HasValue &&
                                application.Job.CompanyId == _currentUserService.CompanyId.Value;

        var isJobCreator = application.Job is not null &&
                           !string.IsNullOrWhiteSpace(application.Job.CreatedByUserId) &&
                           application.Job.CreatedByUserId == _currentUserService.UserId;

        if (!isAdmin && !isJobCreator && !isJobOwnerCompany && application.CandidateId != currentCandidateId)
        {
            throw new ForbiddenAccessException("You are not authorized to view this job application.");
        }

        return MapToDto(application);
    }

    public async Task<IReadOnlyList<JobCandidateApplicationDto>> GetMyApplicationsAsync(int candidateId, CancellationToken cancellationToken = default)
    {
        var applications = await _repository.GetByCandidateIdAsync(candidateId, cancellationToken);
        return applications.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<JobCandidateApplicationDto>> GetJobApplicationsAsync(int jobId, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job is null)
        {
            throw new NotFoundException($"Job with ID {jobId} was not found.");
        }

        if (!_currentUserService.IsAdmin)
        {
            var userCompanyId = _currentUserService.CompanyId;
            if (!userCompanyId.HasValue || userCompanyId.Value != job.CompanyId)
            {
                throw new ForbiddenAccessException("A recruiter cannot view applications for a job belonging to another company.");
            }

            if (!string.IsNullOrWhiteSpace(job.CreatedByUserId) &&
                job.CreatedByUserId != _currentUserService.UserId &&
                !_currentUserService.IsCompanyOwner)
            {
                throw new ForbiddenAccessException("Only the recruiter who opened this job or an administrator can view its applications.");
            }
        }

        var applications = await _repository.GetByJobIdAsync(jobId, cancellationToken);
        return applications.Select(MapToDto).ToList();
    }

    public async Task<JobCandidateApplicationDto> UpdateStatusAsync(int applicationId, JobApplicationStatus status, CancellationToken cancellationToken = default)
    {
        var application = await _repository.GetDetailsByIdAsync(applicationId, cancellationToken);
        if (application is null)
        {
            throw new NotFoundException($"Job application with ID {applicationId} was not found.");
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
        application.UpdateStatus(status);

        _repository.Update(application);
        await _repository.SaveChangesAsync(cancellationToken);

        if (oldStatus != status)
        {
            await _auditService.LogAsync("Application status changed", "JobCandidateApplication", application.Id.ToString(), $"Status changed from {oldStatus} to {status}.", cancellationToken);
        }

        if (oldStatus != status &&
            (status == JobApplicationStatus.UnderReview ||
             status == JobApplicationStatus.InterView ||
             status == JobApplicationStatus.Accepted ||
             status == JobApplicationStatus.Rejected))
        {
            _hangfireService.Enqueue<IEmailNotificationJob>(job =>
                job.SendApplicationStatusChangedNotificationAsync(applicationId, oldStatus, status));
        }

        return MapToDto(application);
    }

    public async Task CancelAsync(int applicationId, int currentUserId, CancellationToken cancellationToken = default)
    {
        var application = await _repository.GetByIdAsync(applicationId, cancellationToken);
        if (application is null)
        {
            throw new NotFoundException($"Job application with ID {applicationId} was not found.");
        }

        if (application.CandidateId != currentUserId)
        {
            throw new ForbiddenAccessException("You are not authorized to cancel this job application.");
        }

        application.Cancel();

        _repository.Update(application);
        await _repository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Application cancelled", "JobCandidateApplication", application.Id.ToString(), $"Application cancelled by candidate {currentUserId}.", cancellationToken);

        _hangfireService.Enqueue<IEmailNotificationJob>(job =>
            job.SendApplicationCancelledNotificationAsync(applicationId));
    }

    private static JobCandidateApplicationDto MapToDto(JobCandidateApplication app)
    {
        return new JobCandidateApplicationDto
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
        };
    }
}
