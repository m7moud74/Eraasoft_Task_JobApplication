using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Command.Applications;

public record CancelApplicationCommand(int ApplicationId, int CurrentUserId) : IRequest<bool>;

public class CancelApplicationCommandHandler : IRequestHandler<CancelApplicationCommand, bool>
{
    private readonly IJobCandidateApplicationRepository _repository;
    private readonly IHangFrieService _hangfireService;
    private readonly IAuditService _auditService;

    public CancelApplicationCommandHandler(
        IJobCandidateApplicationRepository repository,
        IHangFrieService hangfireService,
        IAuditService auditService)
    {
        _repository = repository;
        _hangfireService = hangfireService;
        _auditService = auditService;
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

        // Record audit log
        await _auditService.LogAsync("Application cancelled", "JobCandidateApplication", application.Id.ToString(), $"Application {application.Id} cancelled by candidate {request.CurrentUserId}.", cancellationToken);

        // Enqueue background email notification ONLY after successful database save
        _hangfireService.Enqueue<IEmailNotificationJob>(job => job.SendApplicationCancelledNotificationAsync(request.ApplicationId));

        return true;
    }
}
