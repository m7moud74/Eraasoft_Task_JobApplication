using JobApplication.Application.DTOs;

namespace JobApplication.Application.Interfaces;

public interface ICandidateService
{
    Task<IReadOnlyList<CandidateDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CandidateDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CandidateDto> UpdateAsync(int id, UpdateCandidateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
