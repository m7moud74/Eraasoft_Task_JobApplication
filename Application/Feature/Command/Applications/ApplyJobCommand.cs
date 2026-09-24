using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;

namespace JobApplication.Application.Feature.Command.Applications;

public record ApplyJobCommand(int JobId, int CandidateId) : IRequest<JobCandidateApplicationDto>;

public class ApplyJobCommandHandler : IRequestHandler<ApplyJobCommand, JobCandidateApplicationDto>
{
    private readonly IJobCandidateApplicationRepository _repository;
    private readonly IJobRepository _jobRepository;
    private readonly ICandidateRepository _candidateRepository;
    private readonly IAuditService _auditService;

    public ApplyJobCommandHandler(
        IJobCandidateApplicationRepository repository,
        IJobRepository jobRepository,
        ICandidateRepository candidateRepository,
        IAuditService auditService)
    {
        _repository = repository;
        _jobRepository = jobRepository;
        _candidateRepository = candidateRepository;
        _auditService = auditService;
    }

    public async Task<JobCandidateApplicationDto> Handle(ApplyJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null)
        {
            throw new NotFoundException($"Job with ID {request.JobId} was not found.");
        }

        if (!job.IsActive)
        {
            throw new BadRequestException("Cannot apply to an inactive job.");
        }

        var candidate = await _candidateRepository.GetByIdAsync(request.CandidateId, cancellationToken);
        if (candidate is null)
        {
            throw new NotFoundException($"Candidate with ID {request.CandidateId} was not found.");
        }

        var alreadyApplied = await _repository.ExistsAsync(request.JobId, request.CandidateId, cancellationToken);
        if (alreadyApplied)
        {
            throw new BadRequestException("You have already applied for this job.");
        }

        var application = new JobCandidateApplication
        {
            JobId = request.JobId,
            CandidateId = request.CandidateId,
            JobApplicationStatus = JobApplicationStatus.Applied,
            AppliedAt = DateTime.UtcNow,
            StatusUpdatedAt = DateTime.UtcNow
        };

        await _repository.InsertAsync(application, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        // Record audit log
        await _auditService.LogAsync("Application submitted", "JobCandidateApplication", application.Id.ToString(), $"Candidate {candidate.Id} applied to Job {job.Id} ('{job.Title}').", cancellationToken);

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
}
