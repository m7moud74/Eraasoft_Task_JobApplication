using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Services;

public class CandidateService : ICandidateService
{
    private readonly ICandidateRepository _candidateRepository;
    private readonly ICurrentUserService _currentUserService;

    public CandidateService(ICandidateRepository candidateRepository, ICurrentUserService currentUserService)
    {
        _candidateRepository = candidateRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<CandidateDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var candidates = await _candidateRepository.GetAllAsync(cancellationToken);
        return candidates.Select(MapToDto).ToList();
    }

    public async Task<CandidateDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var candidate = await _candidateRepository.GetByIdAsync(id, cancellationToken);
        if (candidate is null)
        {
            throw new NotFoundException($"Candidate with ID {id} was not found.");
        }

        // Candidates can only view their own profile unless admin
        if (!_currentUserService.IsAdmin && _currentUserService.CandidateId != id)
        {
            throw new ForbiddenAccessException("You are not authorized to view another candidate's profile.");
        }

        return MapToDto(candidate);
    }

    public async Task<CandidateDto> UpdateAsync(int id, UpdateCandidateRequest request, CancellationToken cancellationToken = default)
    {
        var candidate = await _candidateRepository.GetByIdAsync(id, cancellationToken);
        if (candidate is null)
        {
            throw new NotFoundException($"Candidate with ID {id} was not found.");
        }

        if (!_currentUserService.IsCandidate || _currentUserService.CandidateId != id)
        {
            throw new ForbiddenAccessException("Only candidates can update their own profile.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BadRequestException("Candidate name is required.");
        }

        candidate.Name = request.Name.Trim();
        candidate.CvUrl = request.CvUrl?.Trim() ?? string.Empty;

        _candidateRepository.Update(candidate);
        await _candidateRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(candidate);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAdmin && (!_currentUserService.IsCandidate || _currentUserService.CandidateId != id))
        {
            throw new ForbiddenAccessException("You are not authorized to delete this candidate profile.");
        }

        var candidate = await _candidateRepository.GetByIdAsync(id, cancellationToken);
        if (candidate is null)
        {
            throw new NotFoundException($"Candidate with ID {id} was not found.");
        }

        _candidateRepository.Remove(candidate);
        await _candidateRepository.SaveChangesAsync(cancellationToken);
    }

    private static CandidateDto MapToDto(Candidate candidate)
    {
        return new CandidateDto
        {
            Id = candidate.Id,
            Name = candidate.Name,
            Email = candidate.Email,
            CvUrl = candidate.CvUrl
        };
    }
}
