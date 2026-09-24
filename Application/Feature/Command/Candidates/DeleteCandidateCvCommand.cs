using System.Threading;
using System.Threading.Tasks;
using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Command.Candidates;

public record DeleteCandidateCvCommand(int CandidateId) : IRequest<CandidateDto>;

public class DeleteCandidateCvCommandHandler : IRequestHandler<DeleteCandidateCvCommand, CandidateDto>
{
    private readonly ICandidateRepository _candidateRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCandidateCvCommandHandler(
        ICandidateRepository candidateRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService)
    {
        _candidateRepository = candidateRepository;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
    }

    public async Task<CandidateDto> Handle(DeleteCandidateCvCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdmin && _currentUserService.CandidateId != request.CandidateId)
        {
            throw new ForbiddenAccessException("You are only authorized to delete the CV for your own profile.");
        }

        var candidate = await _candidateRepository.GetByIdAsync(request.CandidateId, cancellationToken);
        if (candidate is null)
        {
            throw new NotFoundException($"Candidate with ID {request.CandidateId} was not found.");
        }

        if (!string.IsNullOrWhiteSpace(candidate.CvPublicId))
        {
            await _fileStorageService.DeleteFileAsync(candidate.CvPublicId, cancellationToken);
        }

        candidate.CvUrl = string.Empty;
        candidate.CvPublicId = null;

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
