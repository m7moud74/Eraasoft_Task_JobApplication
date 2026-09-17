using JobApplication.Application.DTOs;

namespace JobApplication.Application.Interfaces;

public interface IJobService
{
    Task<IReadOnlyList<JobDto>> GetAllAsync(bool? activeOnly = null, CancellationToken cancellationToken = default);
    Task<JobDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<JobDto> CreateAsync(CreateJobRequest request, CancellationToken cancellationToken = default);
    Task<JobDto> UpdateAsync(int id, UpdateJobRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
