using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Command.Jobs;

public record DeleteJobCommand(int Id) : IRequest<bool>;

public class DeleteJobCommandHandler : IRequestHandler<DeleteJobCommand, bool>
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobCandidateApplicationRepository _applicationRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteJobCommandHandler(
        IJobRepository jobRepository,
        IJobCandidateApplicationRepository applicationRepository,
        ICurrentUserService currentUserService)
    {
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.Id, cancellationToken);
        if (job is null)
        {
            throw new NotFoundException($"Job with ID {request.Id} was not found.");
        }

        if (!_currentUserService.IsAdmin && job.CreatedByUserId != _currentUserService.UserId)
        {
            throw new ForbiddenAccessException("You are not authorized to delete this job.");
        }

        var hasApplications = await _applicationRepository.AnyByJobIdAsync(request.Id, cancellationToken);
        if (hasApplications)
        {
            throw new BadRequestException($"Cannot delete job with ID {request.Id} because applications have already been submitted for it.");
        }

        _jobRepository.Remove(job);
        await _jobRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
