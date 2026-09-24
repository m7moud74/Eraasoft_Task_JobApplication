using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IRecruiterRepository _recruiterRepository;
    private readonly IJobCandidateApplicationRepository _applicationRepository;
    private readonly ICurrentUserService _currentUserService;

    public JobService(
        IJobRepository jobRepository,
        ICompanyRepository companyRepository,
        IRecruiterRepository recruiterRepository,
        IJobCandidateApplicationRepository applicationRepository,
        ICurrentUserService currentUserService)
    {
        _jobRepository = jobRepository;
        _companyRepository = companyRepository;
        _recruiterRepository = recruiterRepository;
        _applicationRepository = applicationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<JobDto>> GetAllAsync(bool? activeOnly = null, CancellationToken cancellationToken = default)
    {
        var jobs = await _jobRepository.GetAllAsync(cancellationToken);

        if (activeOnly == true)
        {
            jobs = jobs.Where(j => j.IsActive).ToList();
        }

        return jobs.Select(MapToDto).ToList();
    }

    public async Task<JobDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            throw new NotFoundException($"Job with ID {id} was not found.");
        }

        return MapToDto(job);
    }

    public async Task<JobDto> CreateAsync(CreateJobRequest request, CancellationToken cancellationToken = default)
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
            Description = request.Description.Trim(),
            IsActive = request.IsActive,
            CompanyId = company.Id,
            RecruiterId = recruiterId,
            CreatedByUserId = _currentUserService.UserId
        };

        await _jobRepository.InsertAsync(job, cancellationToken);
        await _jobRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(job);
    }

    public async Task<JobDto> UpdateAsync(int id, UpdateJobRequest request, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            throw new NotFoundException($"Job with ID {id} was not found.");
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
        job.Description = request.Description.Trim();
        job.IsActive = request.IsActive;

        _jobRepository.Update(job);
        await _jobRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(job);
    }

    public async Task<JobDto> CloseJobAsync(int id, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            throw new NotFoundException($"Job with ID {id} was not found.");
        }

        if (!_currentUserService.IsAdmin)
        {
            var userCompanyId = _currentUserService.CompanyId;
            if (!userCompanyId.HasValue || userCompanyId.Value != job.CompanyId)
            {
                throw new ForbiddenAccessException("A recruiter cannot close a job belonging to another company.");
            }

            if (!string.IsNullOrWhiteSpace(job.CreatedByUserId) &&
                job.CreatedByUserId != _currentUserService.UserId &&
                !_currentUserService.IsCompanyOwner)
            {
                throw new ForbiddenAccessException("Only the person who opened this job can close it.");
            }
        }

        job.IsActive = false;

        _jobRepository.Update(job);
        await _jobRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(job);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            throw new NotFoundException($"Job with ID {id} was not found.");
        }

        if (!_currentUserService.IsAdmin)
        {
            var userCompanyId = _currentUserService.CompanyId;
            if (!userCompanyId.HasValue || userCompanyId.Value != job.CompanyId)
            {
                throw new ForbiddenAccessException("A recruiter cannot delete a job belonging to another company.");
            }

            if (job.CreatedByUserId != _currentUserService.UserId && !_currentUserService.IsCompanyOwner)
            {
                throw new ForbiddenAccessException("You are not authorized to delete this job.");
            }
        }

        var hasApplications = await _applicationRepository.AnyByJobIdAsync(id, cancellationToken);
        if (hasApplications)
        {
            throw new BadRequestException($"Cannot delete job with ID {id} because applications have already been submitted for it.");
        }

        _jobRepository.Remove(job);
        await _jobRepository.SaveChangesAsync(cancellationToken);
    }

    private static JobDto MapToDto(Job job)
    {
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
            RecruiterName = job.Recruiter?.Name
        };
    }
}
