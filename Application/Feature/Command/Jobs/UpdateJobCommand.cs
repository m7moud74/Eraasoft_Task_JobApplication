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

    public UpdateJobCommandHandler(IJobRepository jobRepository, ICurrentUserService currentUserService)
    {
        _jobRepository = jobRepository;
        _currentUserService = currentUserService;
    }

    public async Task<JobDto> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.Id, cancellationToken);
        if (job is null)
        {
            throw new NotFoundException($"Job with ID {request.Id} was not found.");
        }

        if (!string.IsNullOrWhiteSpace(job.CreatedByUserId) && job.CreatedByUserId != _currentUserService.UserId)
        {
            throw new ForbiddenAccessException("Only the person who opened this job can update or close it.");
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

        return new JobDto
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            IsActive = job.IsActive,
            CreatedByUserId = job.CreatedByUserId
        };
    }
}
