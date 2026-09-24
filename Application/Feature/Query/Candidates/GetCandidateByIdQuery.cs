using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Query.Candidates;

public record GetCandidateByIdQuery(int Id) : IRequest<CandidateDto>;

public class GetCandidateByIdQueryHandler : IRequestHandler<GetCandidateByIdQuery, CandidateDto>
{
    private readonly ICandidateRepository _candidateRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCandidateByIdQueryHandler(ICandidateRepository candidateRepository, ICurrentUserService currentUserService)
    {
        _candidateRepository = candidateRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CandidateDto> Handle(GetCandidateByIdQuery request, CancellationToken cancellationToken)
    {
        var candidate = await _candidateRepository.GetByIdAsync(request.Id, cancellationToken);
        if (candidate is null)
        {
            throw new NotFoundException($"Candidate with ID {request.Id} was not found.");
        }

        if (!_currentUserService.IsAdmin && _currentUserService.CandidateId != request.Id)
        {
            throw new ForbiddenAccessException("You are not authorized to view another candidate's profile.");
        }

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
