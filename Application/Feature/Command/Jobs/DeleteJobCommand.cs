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
    private readonly ICacheService _cacheService;
    private readonly IAuditService _auditService;

    public DeleteJobCommandHandler(
        IJobRepository jobRepository,
        IJobCandidateApplicationRepository applicationRepository,
        ICurrentUserService currentUserService,
        ICacheService cacheService,
        IAuditService auditService)
    {
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _auditService = auditService;
    }

    public async Task<bool> Handle(DeleteJobCommand request, CancellationToken cancellationToken)
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
                throw new ForbiddenAccessException("A recruiter cannot delete a job belonging to another company.");
            }

            if (job.CreatedByUserId != _currentUserService.UserId && !_currentUserService.IsCompanyOwner)
            {
                throw new ForbiddenAccessException("You are not authorized to delete this job.");
            }
        }

        var hasApplications = await _applicationRepository.AnyByJobIdAsync(request.Id, cancellationToken);
        if (hasApplications)
        {
            throw new BadRequestException($"Cannot delete job with ID {request.Id} because applications have already been submitted for it.");
        }

        _jobRepository.Remove(job);
        await _jobRepository.SaveChangesAsync(cancellationToken);

        // Invalidate jobs cache
        await _cacheService.RemoveByPrefixAsync("jobs:", cancellationToken);

        // Record audit log
        await _auditService.LogAsync("Job deleted", "Job", job.Id.ToString(), $"Job '{job.Title}' deleted.", cancellationToken);

        return true;
    }
}
