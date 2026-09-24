using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Command.Candidates;

public record UpdateCandidateCommand(int Id, string Name, string? CvUrl = null) : IRequest<CandidateDto>;

public class UpdateCandidateCommandHandler : IRequestHandler<UpdateCandidateCommand, CandidateDto>
{
    private readonly ICandidateRepository _candidateRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCandidateCommandHandler(
        ICandidateRepository candidateRepository,
        ICurrentUserService currentUserService)
    {
        _candidateRepository = candidateRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CandidateDto> Handle(UpdateCandidateCommand request, CancellationToken cancellationToken)
    {
        var candidate = await _candidateRepository.GetByIdAsync(request.Id, cancellationToken);
        if (candidate is null)
        {
            throw new NotFoundException($"Candidate with ID {request.Id} was not found.");
        }

        if (!_currentUserService.IsCandidate || _currentUserService.CandidateId != request.Id)
        {
            throw new ForbiddenAccessException("Only candidates can update their own profile.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BadRequestException("Candidate name is required.");
        }

        candidate.Name = request.Name.Trim();
        // CV URL can no longer be updated via arbitrary string; it must be uploaded via the CV upload endpoint

        _candidateRepository.Update(candidate);
        await _candidateRepository.SaveChangesAsync(cancellationToken);

        return new CandidateDto
        {
            Id = candidate.Id,
            Name = candidate.Name,
            Email = candidate.Email,
            CvUrl = candidate.CvUrl,
            CvPublicId = candidate.CvPublicId
        };
    }
}
