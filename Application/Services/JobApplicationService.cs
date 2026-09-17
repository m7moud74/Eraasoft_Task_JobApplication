using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;

namespace JobApplication.Application.Services;

public class JobApplicationService : IJobApplicationService
{
    private readonly IJobCandidateApplicationRepository _repository;

    public JobApplicationService(IJobCandidateApplicationRepository repository)
    {
        _repository = repository;
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
}
