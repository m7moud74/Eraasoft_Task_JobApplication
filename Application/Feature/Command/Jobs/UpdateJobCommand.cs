using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Command.Jobs;

public record UpdateJobCommand(int Id, string Title, string Description, bool IsActive) : IRequest<JobDto>;

public class UpdateJobCommandHandler : IRequestHandler<UpdateJobCommand, JobDto>
{
    private readonly IJobRepository _jobRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly IAuditService _auditService;

    public UpdateJobCommandHandler(
        IJobRepository jobRepository,
        ICurrentUserService currentUserService,
        ICacheService cacheService,
        IAuditService auditService)
    {
        _jobRepository = jobRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _auditService = auditService;
    }

    public async Task<JobDto> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.Id, cancellationToken);
        if (job is null)
        {
            throw new NotFoundException($"Job with ID {request.Id} was not found.");
        }

        if (!_currentUserService.IsAdmin)
        {
            var userCompanyId = _currentUserService.CompanyId;
            if (!userCompanyId.HasValue || userCompanyId.Value != job.CompanyId)
            {
                throw new ForbiddenAccessException("A recruiter cannot modify a job belonging to another company.");
            }

            if (!string.IsNullOrWhiteSpace(job.CreatedByUserId) &&
                job.CreatedByUserId != _currentUserService.UserId &&
                !_currentUserService.IsCompanyOwner)
            {
                throw new ForbiddenAccessException("Only the person who opened this job can update or close it.");
            }
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new BadRequestException("Job title is required.");
        }

        job.Title = request.Title.Trim();
        job.Description = request.Description?.Trim() ?? string.Empty;
        job.IsActive = request.IsActive;

        _jobRepository.Update(job);
        await _jobRepository.SaveChangesAsync(cancellationToken);

        // Invalidate jobs cache
        await _cacheService.RemoveByPrefixAsync("jobs:", cancellationToken);

        // Record audit log
        await _auditService.LogAsync("Job updated", "Job", job.Id.ToString(), $"Job '{job.Title}' updated.", cancellationToken);

        return new JobDto
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            IsActive = job.IsActive,
            CreatedByUserId = job.CreatedByUserId,
            CompanyId = job.CompanyId,
            CompanyName = job.Company?.Name,
            RecruiterId = job.RecruiterId,
            RecruiterName = job.Recruiter?.Name,
            CreatedAt = job.CreatedAt
        };
    }
}
