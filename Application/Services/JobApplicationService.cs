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

    public JobApplicationService(
        IJobCandidateApplicationRepository repository,
        IJobRepository jobRepository,
        ICandidateRepository candidateRepository)
    {
        _repository = repository;
        _jobRepository = jobRepository;
        _candidateRepository = candidateRepository;
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

        if (!isAdmin && application.CandidateId != currentCandidateId)
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

        application.UpdateStatus(status);

        _repository.Update(application);
        await _repository.SaveChangesAsync(cancellationToken);

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
