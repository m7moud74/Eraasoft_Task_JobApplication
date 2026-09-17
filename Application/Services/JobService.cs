using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobCandidateApplicationRepository _applicationRepository;

    public JobService(IJobRepository jobRepository, IJobCandidateApplicationRepository applicationRepository)
    {
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
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

        var job = new Job
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            IsActive = request.IsActive
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

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            throw new NotFoundException($"Job with ID {id} was not found.");
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
            IsActive = job.IsActive
        };
    }
}
