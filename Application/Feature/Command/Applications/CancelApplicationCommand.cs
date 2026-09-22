using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Command.Applications;

public record CancelApplicationCommand(int ApplicationId, int CurrentUserId) : IRequest<bool>;

public class CancelApplicationCommandHandler : IRequestHandler<CancelApplicationCommand, bool>
{
    private readonly IJobCandidateApplicationRepository _repository;

    public CancelApplicationCommandHandler(IJobCandidateApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(CancelApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _repository.GetByIdAsync(request.ApplicationId, cancellationToken);
        if (application is null)
        {
            throw new NotFoundException($"Job application with ID {request.ApplicationId} was not found.");
        }

        if (application.CandidateId != request.CurrentUserId)
        {
            throw new ForbiddenAccessException("Only the candidate who submitted this application can cancel it.");
        }

        application.Cancel();

        _repository.Update(application);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
