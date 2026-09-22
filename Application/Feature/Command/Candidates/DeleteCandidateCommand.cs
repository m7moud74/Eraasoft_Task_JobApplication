using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Command.Candidates;

public record DeleteCandidateCommand(int Id) : IRequest<bool>;

public class DeleteCandidateCommandHandler : IRequestHandler<DeleteCandidateCommand, bool>
{
    private readonly ICandidateRepository _candidateRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCandidateCommandHandler(
        ICandidateRepository candidateRepository,
        ICurrentUserService currentUserService)
    {
        _candidateRepository = candidateRepository;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteCandidateCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdmin && (!_currentUserService.IsCandidate || _currentUserService.CandidateId != request.Id))
        {
            throw new ForbiddenAccessException("You are not authorized to delete this candidate profile.");
        }

        var candidate = await _candidateRepository.GetByIdAsync(request.Id, cancellationToken);
        if (candidate is null)
        {
            throw new NotFoundException($"Candidate with ID {request.Id} was not found.");
        }

        _candidateRepository.Remove(candidate);
        await _candidateRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
