using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;

namespace JobApplication.Application.Feature.Command.Jobs;

public record CreateJobCommand(string Title, string Description, bool IsActive, int? CompanyId = null) : IRequest<JobDto>;

public class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, JobDto>
{
    private readonly IJobRepository _jobRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IRecruiterRepository _recruiterRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly IAuditService _auditService;

    public CreateJobCommandHandler(
        IJobRepository jobRepository,
        ICompanyRepository companyRepository,
        IRecruiterRepository recruiterRepository,
        ICurrentUserService currentUserService,
        ICacheService cacheService,
        IAuditService auditService)
    {
        _jobRepository = jobRepository;
        _companyRepository = companyRepository;
        _recruiterRepository = recruiterRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _auditService = auditService;
    }

    public async Task<JobDto> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new BadRequestException("Job title is required.");
        }

        int targetCompanyId;
        int? recruiterId = _currentUserService.RecruiterId;

        if (_currentUserService.IsAdmin)
        {
            if (request.CompanyId.HasValue && request.CompanyId.Value > 0)
            {
                targetCompanyId = request.CompanyId.Value;
            }
            else if (_currentUserService.CompanyId.HasValue)
            {
                targetCompanyId = _currentUserService.CompanyId.Value;
            }
            else
            {
                throw new BadRequestException("CompanyId must be specified when creating a job as an administrator.");
            }
        }
        else
        {
            if (!_currentUserService.CompanyId.HasValue)
            {
                throw new ForbiddenAccessException("You must belong to a company to create jobs.");
            }

            targetCompanyId = _currentUserService.CompanyId.Value;

            if (request.CompanyId.HasValue && request.CompanyId.Value != targetCompanyId)
            {
                throw new ForbiddenAccessException("You cannot create jobs for another company.");
            }

            if (!recruiterId.HasValue && !string.IsNullOrWhiteSpace(_currentUserService.UserId))
            {
                var recruiter = await _recruiterRepository.GetByUserIdAsync(_currentUserService.UserId, cancellationToken);
                recruiterId = recruiter?.Id;
            }
        }

        var company = await _companyRepository.GetByIdAsync(targetCompanyId, cancellationToken);
        if (company is null)
        {
            throw new NotFoundException($"Company with ID {targetCompanyId} was not found.");
        }

        if (company.Status != CompanyStatus.Approved)
        {
            throw new ForbiddenAccessException($"Cannot post jobs for a company with status '{company.Status}'. The company must be approved first.");
        }

        var job = new Job
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            IsActive = request.IsActive,
            CompanyId = company.Id,
            RecruiterId = recruiterId,
            CreatedByUserId = _currentUserService.UserId
        };

        await _jobRepository.InsertAsync(job, cancellationToken);
        await _jobRepository.SaveChangesAsync(cancellationToken);

        // Invalidate jobs cache
        await _cacheService.RemoveByPrefixAsync("jobs:", cancellationToken);

        // Record audit log
        await _auditService.LogAsync("Job created", "Job", job.Id.ToString(), $"Job '{job.Title}' created for Company {company.Id}.", cancellationToken);

        return new JobDto
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            IsActive = job.IsActive,
            CreatedByUserId = job.CreatedByUserId,
            CompanyId = company.Id,
            CompanyName = company.Name,
            RecruiterId = recruiterId,
            CreatedAt = job.CreatedAt
        };
    }
}
