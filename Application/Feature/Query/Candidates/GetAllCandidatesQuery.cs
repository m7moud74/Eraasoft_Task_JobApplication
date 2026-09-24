using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Query.Candidates;

public record GetAllCandidatesQuery : IRequest<IReadOnlyList<CandidateDto>>;

public class GetAllCandidatesQueryHandler : IRequestHandler<GetAllCandidatesQuery, IReadOnlyList<CandidateDto>>
{
    private readonly ICandidateRepository _candidateRepository;

    public GetAllCandidatesQueryHandler(ICandidateRepository candidateRepository)
    {
        _candidateRepository = candidateRepository;
    }

    public async Task<IReadOnlyList<CandidateDto>> Handle(GetAllCandidatesQuery request, CancellationToken cancellationToken)
    {
        var candidates = await _candidateRepository.GetAllAsync(cancellationToken);
        return candidates.Select(candidate => new CandidateDto
        {
            Id = candidate.Id,
            Name = candidate.Name,
            Email = candidate.Email,
            CvUrl = candidate.CvUrl,
            CvPublicId = candidate.CvPublicId
        }).ToList();
    }
}
