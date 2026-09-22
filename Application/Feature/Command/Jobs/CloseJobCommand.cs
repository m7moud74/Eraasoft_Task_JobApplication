using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Command.Jobs;

public record CloseJobCommand(int Id) : IRequest<JobDto>;

public class CloseJobCommandHandler : IRequestHandler<CloseJobCommand, JobDto>
{
    private readonly IJobRepository _jobRepository;
    private readonly ICurrentUserService _currentUserService;

    public CloseJobCommandHandler(IJobRepository jobRepository, ICurrentUserService currentUserService)
    {
        _jobRepository = jobRepository;
        _currentUserService = currentUserService;
    }

    public async Task<JobDto> Handle(CloseJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.Id, cancellationToken);
        if (job is null)
        {
            throw new NotFoundException($"Job with ID {request.Id} was not found.");
        }

        if (string.IsNullOrWhiteSpace(job.CreatedByUserId) || job.CreatedByUserId != _currentUserService.UserId)
        {
            throw new ForbiddenAccessException("Only the person who opened this job can close it.");
        }

        job.IsActive = false;

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
